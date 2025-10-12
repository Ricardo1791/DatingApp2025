﻿using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController(IMessageRepository messageRepo, IMemberRepository memberRepo): BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var sender = await memberRepo.GetMemberByIdAsync(User.GetMemberId());
            var recipient = await memberRepo.GetMemberByIdAsync(createMessageDto.RecipientId);

            if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId) return BadRequest("Cannot send this message");

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDto.Content,

            };

            messageRepo.AddMessage(message);

            if (await messageRepo.SaveAllAsync()) return message.ToDto();

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer([FromQuery] MessageParams messageParams)
        {
            messageParams.MemberId = User.GetMemberId();

            return await messageRepo.GetMessagesForMember(messageParams);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
        {
            return Ok(await messageRepo.GetMessageThread(User.GetMemberId(), recipientId));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(string id)
        {
            var memberId = User.GetMemberId();

            var message = await messageRepo.GetMessage(id);

            if (message == null) return BadRequest("Cannot delete this message");

            if (message.SenderId != memberId && message.RecipientId != memberId)
                return BadRequest("You cannot delete this message");

            if (message.SenderId == memberId) message.SenderDeleted = true;
            if (message.RecipientId == memberId) message.RecipentDeleted = true;

            if (message is { SenderDeleted: true, RecipentDeleted: true })
            {
                messageRepo.DeleteMessage(message);
            }

            if (await messageRepo.SaveAllAsync()) return Ok();

            return BadRequest("Cannot delete the message");
        }
    }
}
