namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Contracts
{
    public interface IAuthorizationService
    {
        Task<bool> CheckAccessAsync(CheckAccessDto check);

        public record CheckAccessDto(
            string UserId,
            string Relation,
            string ResourceType,
            string ResourceId
        );
    }
}
