using System;
using Newtonsoft.Json;

namespace Neuro_Plateup
{
    public class NeuroAPI
    {
        [Serializable]
        public class GameName
        {
            public readonly string game = "PlateUp!";
        }

        [Serializable]
        public class Startup : GameName
        {
            public readonly string command = "startup";
        }

        [Serializable]
        public class ContextData
        {
            public string message;
            public bool silent;
        }

        [Serializable]
        public class Context : GameName
        {
            public readonly string command = "context";
            public ContextData data;
        }

        [Serializable]
        public class Action
        {
            public string name;
            public string description;

            [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
            public JsonSchema schema;
        }

        [Serializable]
        public class RegisterActionsData
        {
            public Action[] actions;
        }

        [Serializable]
        public class RegisterActions : GameName
        {
            public readonly string command = "actions/register";
            public RegisterActionsData data;
        }

        [Serializable]
        public class UnregisterActionsData
        {
            public string[] action_names;
        }

        [Serializable]
        public class UnregisterActions : GameName
        {
            public readonly string command = "actions/unregister";
            public UnregisterActionsData data;
        }

        [Serializable]
        public class ForceActionData
        {
            public string state;
            public string query;
            public bool ephemeral_context = false;
            public string[] action_names;
        }

        [Serializable]
        public class ForceAction : GameName
        {
            public readonly string command = "action/force";
            public ForceActionData data;
        }

        [Serializable]
        public class ActionResultData
        {
            public string id;
            public bool success;
            public string message;
        }

        [Serializable]
        public class ActionResult : GameName
        {
            public readonly string command = "action/result";
            public ActionResultData data;
        }

        [Serializable]
        public class AnswerData
        {
            public string id;
            public string name;
            public string Data { get; set; }
        }

        [Serializable]
        public class Answer
        {
            public string command;
            public AnswerData data;
        }
    }
}