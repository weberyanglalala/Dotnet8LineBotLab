using Dotnet8LineBotLab.Services;
using isRock.LineBot;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet8LineBotLab.Controllers;

public class DifyChatController : LineWebHookControllerBase
{
    private readonly string _adminUserId;
    private readonly Bot _bot;
    private readonly DifyService _difyService;
    private readonly CacheService _cacheService;

    public DifyChatController(DifyService difyService, CacheService cacheService)
    {
        _difyService = difyService;
        _cacheService = cacheService;
        _adminUserId = "";
        // 移至定義 LineWebHookControllerBase
        ChannelAccessToken =
            "";
        _bot = new Bot(ChannelAccessToken);
    }

    [Route("api/dify/LineBotChatWebHook")]
    [HttpPost]
    public async Task<IActionResult> GetChatResult()
    {
        try
        {
            if (IsLineVerify()) return Ok();
            foreach (var lineEvent in ReceivedMessage.events)
            {
                if (lineEvent != null && lineEvent.type.ToLower() == "message" && lineEvent.message.type == "text")
                {
                    var lineUserId = lineEvent.source.userId;
                    var user = GetUserInfo(lineUserId);
                    _bot.DisplayLoadingAnimation(lineEvent.source.userId, 20);
                    var conversationId = _cacheService.GetCache(lineEvent.source.userId);

                    string responseMessage;
                    //如果用戶輸入 /forget 則把 conversationId 清空，重啟對話
                    if (lineEvent.message.text.Trim().ToLower() == "/forget")
                    {
                        _cacheService.RemoveCache(lineEvent.source.userId);
                        responseMessage = "我已經忘記之前所有對話了";
                    }
                    else
                    {
                        var requestData = new
                        {
                            inputs = new { },
                            query = lineEvent.message.text,
                            response_mode = "streaming",
                            conversation_id = string.IsNullOrEmpty(conversationId) ? "" : conversationId.ToString(),
                            user = user.userId
                        };
                        var response = await _difyService.GetChatCompletionAsync(requestData);
                        responseMessage = response.Message;
                        //儲存對話ID(to cache)
                        _cacheService.SetCache(lineEvent.source.userId, response.ConversationId);
                    }

                    _bot.ReplyMessage(lineEvent.replyToken, responseMessage);
                }
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