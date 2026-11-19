namespace Test_Taste_Console_Application.Domain.Services.Interfaces
{
    ///<summary>
    /// An output service that writes data from the Solar System OpenData API<see href="https://api.le-systeme-solaire.net/"/> to a file on disk. 
    ///</summary>
    public interface IFileOutputService
    {
        void OutputAllPlanetsAndTheirAverageMoonTemperatureToFile();
    }
}
