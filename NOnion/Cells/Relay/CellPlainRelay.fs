﻿namespace NOnion.Cells.Relay

open System.IO
open System.Security.Cryptography

open NOnion
open NOnion.Cells
open NOnion.Utility

type OptionAlias<'a> =
    Option<'a>

type EndReason =
    | Misc = 1uy
    | ResolveFailed = 2uy
    | ConnectionRefused = 3uy
    | ExitPolicy = 4uy
    | Destroy = 5uy
    | Done = 6uy
    | Timeout = 7uy
    | NoRoute = 8uy
    | Hibernating = 9uy
    | Internal = 10uy
    | ResourceLimit = 11uy
    | ConnectionReset = 12uy
    | TorProtocolViolation = 13uy
    | NotDirectory = 14uy

type RelayData =
    | RelayBegin
    | RelayData of Data: array<byte>
    | RelayEnd of EndReason
    | RelayConnected of Data: array<byte>
    | RelaySendMe
    | RelayExtend
    | RelayExtended
    | RelayTruncate
    | RelayTruncated of Reason: byte
    | RelayDrop
    | RelayResolve
    | RelayResolved
    | RelayBeginDirectory
    | RelayExtend2 of RelayExtend2
    | RelayExtended2 of RelayExtended2

    static member FromBytes (command: byte) (data: array<byte>) =
        use memStream = new MemoryStream (data)
        use reader = new BinaryReader (memStream)

        match command with
        | RelayCommands.RelayData -> RelayData data
        | RelayCommands.RelayEnd ->
            reader.ReadByte ()
            |> LanguagePrimitives.EnumOfValue<byte, EndReason>
            |> RelayEnd
        | RelayCommands.RelayConnected -> RelayConnected data
        | RelayCommands.RelayTruncated -> reader.ReadByte () |> RelayTruncated
        | RelayCommands.RelayExtended2 ->
            RelayExtended2.FromBytes reader |> RelayExtended2
        | _ -> failwith "Unsupported command"

    member self.GetCommand () : byte =
        match self with
        | RelayBeginDirectory -> RelayCommands.RelayBeginDirectory
        | RelayData _ -> RelayCommands.RelayData
        | RelaySendMe _ -> RelayCommands.RelaySendMe
        | RelayExtend2 _ -> RelayCommands.RelayExtend2
        | _ -> failwith "Not implemeted yet"

    member self.ToBytes () =
        match self with
        | RelayData data -> data
        | RelaySendMe _ -> Array.zeroCreate 3
        | RelayExtend2 extend2 -> extend2.ToBytes ()
        | _ -> Array.zeroCreate 0

type CellPlainRelay =
    {
        Recognized: uint16
        StreamId: uint16
        Digest: array<byte>
        Data: RelayData
        Padding: array<byte>
    }

    static member Create
        (streamId: uint16)
        (data: RelayData)
        (digest: array<byte>)
        =
        let payload = data.ToBytes ()

        let paddingLen = Constants.MaximumRelayPayloadLength - payload.Length

        let padding = Array.zeroCreate<byte> paddingLen

        RandomNumberGenerator
            .Create()
            .GetNonZeroBytes padding

        Array.fill
            padding
            0
            (min padding.Length Constants.PaddingZeroPrefixLength)
            0uy

        {
            Recognized = 0us
            StreamId = streamId
            Data = data
            Digest = digest
            Padding = padding
        }

    static member FromBytes (bytes: array<byte>) =
        use memStream = new MemoryStream (bytes)
        use reader = new BinaryReader (memStream)
        let relayCommand = reader.ReadByte ()
        let recognized = BinaryIO.ReadBigEndianUInt16 reader
        let streamId = BinaryIO.ReadBigEndianUInt16 reader
        let digest = reader.ReadBytes Constants.RelayDigestLength

        let data =
            BinaryIO.ReadBigEndianUInt16 reader |> int |> reader.ReadBytes

        let padding =
            reader.ReadBytes (Constants.MaximumRelayPayloadLength - data.Length)

        {
            Recognized = recognized
            StreamId = streamId
            Digest = digest
            Data = RelayData.FromBytes relayCommand data
            Padding = padding
        }

    member self.ToBytes (emptyDigest: bool) : array<byte> =
        use memStream = new MemoryStream (Constants.FixedPayloadLength)
        use writer = new BinaryWriter (memStream)
        self.Data.GetCommand () |> writer.Write

        self.Recognized |> BinaryIO.WriteUInt16BigEndian writer

        self.StreamId |> BinaryIO.WriteUInt16BigEndian writer

        let digest =
            if emptyDigest then
                Array.zeroCreate<byte> Constants.RelayDigestLength
            else
                self.Digest

        digest |> writer.Write

        self.Data.ToBytes ()
        |> Array.length
        |> uint16
        |> BinaryIO.WriteUInt16BigEndian writer

        self.Data.ToBytes () |> writer.Write
        self.Padding |> writer.Write

        memStream.ToArray ()
