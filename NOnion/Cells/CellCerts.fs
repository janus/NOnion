﻿namespace NOnion.Cells

open System.IO

open NOnion
open NOnion.Extensions.BinaryIOExtensions

type Cert =
    {
        Type: byte
        Certificate: array<byte>
    }

type CellCerts =
    {
        Certs: seq<Cert>
    }

    static member Deserialize (reader: BinaryReader) =

        let certificatesCount = reader.ReadByte () |> int

        let rec readCertificates certificates n =
            if n = 0 then
                certificates
            else
                let certificate =
                    {
                        Cert.Type = reader.ReadByte ()
                        Cert.Certificate =
                            reader.ReadBigEndianUInt16 ()
                            |> int
                            |> reader.ReadBytes
                    }

                readCertificates (certificates @ [ certificate ]) (n - 1)

        let certs = readCertificates List.empty certificatesCount

        {
            Certs = certs
        }
        :> ICell

    interface ICell with

        member self.Command = 129uy

        member self.Serialize writer =
            let rec writeCertificates (certificates: seq<Cert>) =
                if Seq.isEmpty certificates then
                    ()
                else
                    let certificate = Seq.head certificates

                    writer.Write certificate.Type

                    certificate.Certificate.Length
                    |> uint16
                    |> writer.WriteUInt16BigEndian

                    writer.Write certificate.Certificate

                    writeCertificates (Seq.tail certificates)

            self.Certs |> Seq.length |> uint8 |> writer.Write

            writeCertificates self.Certs