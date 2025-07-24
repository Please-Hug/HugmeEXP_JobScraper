namespace JobScraper.Core.Interfaces;

public interface IKakaoMapService
{
    Task<Tuple<decimal, decimal>> GetCoordinatesAsync(string address);
}