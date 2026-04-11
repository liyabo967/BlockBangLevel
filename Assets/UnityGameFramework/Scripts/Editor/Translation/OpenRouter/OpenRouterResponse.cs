using System;
using System.Collections.Generic;

namespace UnityGameFramework.Scripts.Editor.Translation.OpenRouter
{
    [Serializable]
    public class OpenRouterResponse
    {
        public string id;
        public string provider;
        public string model;
        public string @object;
        public long created;
        public List<OpenRouterChoice> choices;
        public string system_fingerprint;
        public OpenRouterUsage usage;
    }
    
    [Serializable]
    public class OpenRouterChoice
    {
        public int index;
        public string finish_reason;
        public string native_finish_reason;
        public object logprobs;
        public OpenRouterMessage message;
    }
    
    [Serializable]
    public class OpenRouterMessage
    {
        public string role;
        public string content;
        public object refusal;
        public string reasoning;
        public List<OpenRouterReasoningDetail> reasoning_details;
    }
    
    [Serializable]
    public class OpenRouterReasoningDetail
    {
        public string format;
        public int index;
        public string type;
        public string text;
    }
    
    [Serializable]
    public class OpenRouterUsage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
        public float cost;
        public bool is_byok;

        public PromptTokensDetails prompt_tokens_details;
        public CompletionTokensDetails completion_tokens_details;
        public CostDetails cost_details;
    }
    
    [Serializable]
    public class PromptTokensDetails
    {
        public int cached_tokens;
        public int audio_tokens;
    }
    
    [Serializable]
    public class CompletionTokensDetails
    {
        public int reasoning_tokens;
        public int audio_tokens;
    }
    
    [Serializable]
    public class CostDetails
    {
        public float upstream_inference_cost;
        public float upstream_inference_prompt_cost;
        public float upstream_inference_completions_cost;
    }
}