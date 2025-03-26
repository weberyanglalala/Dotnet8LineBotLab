using isRock.LineBot;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet8LineBotLab.Controllers;

public class ChatController : LineWebHookControllerBase
{
    private readonly string _adminUserId;
    private readonly Bot _bot;

    public ChatController()
    {
        _adminUserId = "";
        // 移至定義 LineWebHookControllerBase
        ChannelAccessToken =
            "";
        _bot = new Bot(ChannelAccessToken);
    }

    [HttpPost]
    [Route("api/LineBotChatWebHook")]
    public async Task<IActionResult> GetChatResult()
    {
        try
        {
            if (IsLineVerify()) return Ok();
            foreach (var lineEvent in ReceivedMessage.events)
            {
                var lineUserId = lineEvent.source.userId;
                var user = GetUserInfo(lineUserId);
                _bot.DisplayLoadingAnimation(lineEvent.source.userId, 20);
                var responseMessage = $"hello, {user.displayName} {user.statusMessage}, type: {lineEvent.message.type}";
                _bot.ReplyMessage(lineEvent.replyToken, responseMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            _bot.PushMessage(_adminUserId, "系統忙碌中，請稍後再試。");
            return Ok();
        }

        return Ok();
    }

    private bool IsLineVerify()
    {
        return ReceivedMessage.events == null || ReceivedMessage.events.Count() <= 0 ||
               ReceivedMessage.events.FirstOrDefault().replyToken == "00000000000000000000000000000000";
    }
}