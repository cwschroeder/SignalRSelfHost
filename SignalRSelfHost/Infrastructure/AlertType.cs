namespace SignalRSelfHost.Infrastructure
{
    public enum AlertType
    {
        UnzipDirMissing,
        CsvFileSizeNotGrown,
        CsvMinDateIncreased,
        CsvMaxDateDecreased,

        NoUploadWithinExpectedInterval
    }
}