using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace SpringHackathon.Models
{
    [CollectionName("Roles")]
    public class UserRole : MongoIdentityRole<Guid>
    {
        
    }
}
