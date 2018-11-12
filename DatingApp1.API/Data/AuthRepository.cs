using System;
using System.Threading.Tasks;
using DatingApp1.API.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DatingApp1.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            this._context=context;   
        }
        public async Task<User> Login(string username, string password)
        {
            var user= await _context.Users.FirstOrDefaultAsync(x => x.Name == username);

            if( user == null)
                return null;

            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;
            
            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (HMACSHA512 hma = new HMACSHA512(passwordSalt))
            {
                byte[] computedHash=hma.ComputeHash(Encoding.UTF8.GetBytes(password));
                for( int i=0;i<computedHash.Length;i++)
                {
                    if(computedHash[i] != passwordHash[i])
                        return false;
                }      
            }   
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash=passwordHash;
            user.PasswordSalt=passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using( HMACSHA512 hmac= new HMACSHA512())
            {
            passwordSalt=hmac.Key;
            passwordHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if( await _context.Users.AnyAsync( x => x.Name==username))
                return true;  

            return false;
        }
    }
}