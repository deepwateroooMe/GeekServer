using System.Text;
using System.Text.Json;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Utils;
using Microsoft.AspNetCore.Http;
using NLog;
namespace Geek.Server.Core.Net.Http {

    internal class HttpHandler {

        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public static async Task HandleRequest(HttpContext context) { // 怎么处理网络请求的 ?
            try {
                string ip = context.Connection.RemoteIpAddress.ToString();
                string url = context.Request.PathBase + context.Request.Path;
                LOGGER.Info("收到来自[{}]的HTTP请求. 请求url:[{}]", ip, url);

                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                foreach (var keyValuePair in context.Request.Query)
                    paramMap.Add(keyValuePair.Key, keyValuePair.Value[0]);

                context.Response.Headers.Add("content-type", "text/html;charset=utf-8");

                if (context.Request.Method.Equals("POST")) { // POST 请求
                    var headCType = context.Request.ContentType;
                    if (string.IsNullOrEmpty(headCType)) {
                        await context.Response.WriteAsync("http header content type is null");
                        return;
                    }
                    var isJson = context.Request.HasJsonContentType();
                    var isForm = context.Request.HasFormContentType;
                    Console.WriteLine("isJson:" + isJson);
                    if (isJson) {
                        JsonElement json = await context.Request.ReadFromJsonAsync<JsonElement>();
                        foreach (var keyValuePair in json.EnumerateObject()) {
                            if (paramMap.ContainsKey(keyValuePair.Name)) {
                                await context.Response.WriteAsync(new HttpResult(HttpResult.Stauts.ParamErr, "参数重复了:" + keyValuePair.Name));
                                return;
                            }
                            var key = keyValuePair.Name;              // 要这两行作什么用呢?
                            var val = keyValuePair.Value.GetString(); // 要这两行作什么用呢?
                            paramMap.Add(keyValuePair.Name, keyValuePair.Value.GetString()); // 这里又再写一遍,是因为不步及任何的类型转换吗?
                        }
                    } else if (isForm) {
                        foreach (var keyValuePair in context.Request.Form) {
                            if (paramMap.ContainsKey(keyValuePair.Key)) {
                                await context.Response.WriteAsync(new HttpResult(HttpResult.Stauts.ParamErr, "参数重复了:" + keyValuePair.Key));
                                return;
                            }
                            paramMap.Add(keyValuePair.Key, keyValuePair.Value[0]);
                        }
                    }
                }
                var str = new StringBuilder();
                str.Append("请求参数:");
                foreach (var parameter in paramMap) {
                    if (parameter.Key.Equals(""))
                        continue;
                    str.Append("'").Append(parameter.Key).Append("'='").Append(parameter.Value).Append("'  ");
                }
                LOGGER.Info(str.ToString());
                if (!paramMap.TryGetValue("command", out var cmd)) {
                    await context.Response.WriteAsync(HttpResult.Undefine);
                    return;
                }
                if (!Settings.AppRunning) { // 服务器 没有 在运行, 服务器 状态错误
                    await context.Response.WriteAsync(new HttpResult(HttpResult.Stauts.ActionFailed, "服务器状态错误[正在启/关服]"));
                    return;
                }
                var handler = HotfixMgr.GetHttpHandler(cmd); // 去看一下 ?
                if (handler == null) {
                    LOGGER.Warn($"http cmd handler 不存在：{cmd}");
                    await context.Response.WriteAsync(HttpResult.Undefine);
                    return;
                }
                // 验证
                var checkCode = handler.CheckSgin(paramMap);
                if (!string.IsNullOrEmpty(checkCode)) { // 如果MD5　验证码非空（大概是说，有热更新之类的？），就异步把MD5码结果写回去
                    await context.Response.WriteAsync(checkCode);
                    return;
                }

                var ret = await Task.Run(() => { return handler.Action(ip, url, paramMap); }); // <<<<<<<<<< 这里是 筛选过后的 真正的执行
                LOGGER.Warn("http result:" + ret);
                await context.Response.WriteAsync(ret);
            }
            catch (Exception e) {
                LOGGER.Error("执行http异常. {} {}", e.Message, e.StackTrace);
                await context.Response.WriteAsync(e.Message);
            }
        }
    }
}
