using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace SpringHackathon.Models
{
    [CollectionName("Users")]
    public class User : MongoIdentityUser<Guid>
    {
    }
}
