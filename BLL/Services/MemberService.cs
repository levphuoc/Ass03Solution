using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            await _memberRepository.AddAsync(member);
            /*await _hubContext.Clients.All.SendAsync("ReceiveUpdate");*/
        }

        public async Task UpdateMemberAsync(Member member)
        {
            await _memberRepository.UpdateAsync(member);
            /*await _hubContext.Clients.All.SendAsync("ReceiveUpdate");*/
        }

        public async Task DeleteMemberAsync(int id)
        {
            await _memberRepository.DeleteAsync(id);
            /*await _hubContext.Clients.All.SendAsync("ReceiveUpdate");*/
        }
    }
}
