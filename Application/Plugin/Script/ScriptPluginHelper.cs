﻿using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jint.Native;
using SharedLibraryCore.Interfaces;

namespace IW4MAdmin.Application.Plugin.Script;

public class ScriptPluginHelper
{
    private readonly IManager _manager;
    private readonly ScriptPluginV2 _scriptPlugin;
    private readonly SemaphoreSlim _onRequestRunning = new(1, 5);
    private const int RequestTimeout = 500;

    public ScriptPluginHelper(IManager manager, ScriptPluginV2 scriptPlugin)
    {
        _manager = manager;
        _scriptPlugin = scriptPlugin;
    }

    public void GetUrl(string url, Delegate callback)
    {
        RequestUrl(new ScriptPluginWebRequest(url), callback);
    }

    public void PostUrl(string url, Delegate callback)
    {
        RequestUrl(new ScriptPluginWebRequest(url, null, "POST"), callback);
    }

    public void RequestUrl(ScriptPluginWebRequest request, Delegate callback)
    {
        Task.Run(() =>
        {
            try
            {
                var response = RequestInternal(request);
                _scriptPlugin.ExecuteWithErrorHandling(scriptEngine =>
                {
                    callback.DynamicInvoke(JsValue.Undefined, new[] { JsValue.FromObject(scriptEngine, response) });
                });
            }
            catch
            {
                // ignored
            }
        });
    }

    public void RequestNotify(int delayMs, Delegate callback)
    {
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delayMs, _manager.CancellationToken);
                _scriptPlugin.ExecuteWithErrorHandling(_ => callback.DynamicInvoke(JsValue.Undefined));
            }
            catch
            {
                //  ignored
            }
        });
    }

    private object RequestInternal(ScriptPluginWebRequest request)
    {
        using var tokenSource = new CancellationTokenSource(RequestTimeout);
        _onRequestRunning.Wait(tokenSource.Token);

        using var client = new HttpClient();

        try
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), request.Url);

            if (request.Body is not null)
            {
                requestMessage.Content = new StringContent(request.Body.ToString() ?? string.Empty, Encoding.Default,
                    request.ContentType ?? "text/plain");
            }

            if (request.Headers is not null)
            {
                foreach (var (key, value) in request.Headers)
                {
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        requestMessage.Headers.Add(key, value);
                    }
                }
            }

            var response = client.Send(requestMessage, tokenSource.Token);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            return reader.ReadToEnd();
        }
        catch (HttpRequestException ex)
        {
            return new
            {
                ex.StatusCode,
                ex.Message,
                IsError = true
            };
        }
        catch (Exception ex)
        {
            return new
            {
                ex.Message,
                IsError = true
            };
        }
        finally
        {
            _onRequestRunning.Release();
        }
    }
}
