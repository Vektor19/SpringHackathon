using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SpringHackathon.Models
{
    [CollectionName("Users")]
    public class User : MongoIdentityUser<Guid>
    {
    }
}
