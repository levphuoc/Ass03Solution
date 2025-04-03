using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IHubContext<MemberHub> _hubContext;

        public MemberService(IMemberRepository memberRepository, IHubContext<MemberHub> hubContext)
        {
            _memberRepository = memberRepository;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<Member>> GetMembersAsync()
        {
            return await _memberRepository.GetAllAsync();
        }

        public async Task<Member> GetMemberByIdAsync(int id)
        {
            return await _memberRepository.GetByIdAsync(id);
        }

        public async Task AddMemberAsync(Member member)
        {
            ValidateMember(member);
            await CheckEmailUniqueness(member.Email);

            await _memberRepository.AddAsync(member);
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");
        }

        public async Task UpdateMemberAsync(Member member)
        {
            ValidateMember(member);

            // Get existing member to check if email is being changed
            var existingMember = await _memberRepository.GetByIdAsync(member.MemberId);
            if (existingMember == null)
            {
                throw new ArgumentException("Member not found");
            }

            // Only check email uniqueness if the email is being changed
            if (!string.Equals(existingMember.Email, member.Email, StringComparison.OrdinalIgnoreCase))
            {
                await CheckEmailUniqueness(member.Email);
            }

            await _memberRepository.UpdateAsync(member);
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");
        }

        public async Task DeleteMemberAsync(int id)
        {
            await _memberRepository.DeleteAsync(id);
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");
        }

        private void ValidateMember(Member member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            // Validate email format (including Gmail check)
            if (!IsValidEmail(member.Email))
            {
                throw new ArgumentException("Invalid email format. Please provide a valid Gmail address.");
            }

            // Validate password length
            if (string.IsNullOrWhiteSpace(member.Password) || member.Password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters long.");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Check basic email format
                if (!Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase))
                {
                    return false;
                }

                // Check if it's a Gmail address
                var emailParts = email.Split('@');
                if (emailParts.Length != 2)
                    return false;

                var domain = emailParts[1].ToLower();
                return domain == "gmail.com";
            }
            catch
            {
                return false;
            }
        }

        private async Task CheckEmailUniqueness(string email)
        {
            var allMembers = await _memberRepository.GetAllAsync();
            if (allMembers.Any(m => string.Equals(m.Email, email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Email '{email}' is already in use. Please use a different email.");
            }
        }

        public async Task<bool> UpdateProfileAsync(int memberId, string companyName, string city, string email, string password)
        {
            try
            {
                // Get the current member
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    return false;
                }

                // Update the fields
                member.CompanyName = companyName;
                member.City = city;
                member.Email = email;
                
                // Only update password if it's provided
                if (!string.IsNullOrWhiteSpace(password))
                {
                    member.Password = password;
                }

                // Save changes
                await _memberRepository.UpdateAsync(member);
                
                // Notify clients if needed
                // await _hubContext.Clients.All.SendAsync("ReceiveUpdate");
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Member>> SearchMembersAsync(string email, string companyName)
        {
            return await _memberRepository.SearchAsync(email, companyName);
        }
    }
}