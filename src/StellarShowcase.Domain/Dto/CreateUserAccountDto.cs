using System;

namespace StellarShowcase.Domain.Dto
{
    public class CreateUserAccountDto
    {
        public string FullName { get; set; }
        public string FullAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Passphrase { get; set; }
    }

    public class UserAccountDto : CreateUserAccountDto
    {
        public Guid Id { get; set; }
        public AccountDto Account { get; set; }
        public string AccountId { get; set; }
    }
}
