using System;
using System.Collections.Generic;
using Unity.Entities;

namespace Neuro_Plateup
{
    public class BotAction
    {
        public string Name;
        public GameState RequiredState;
        public BotRole Roles;
        public Func<ActionReporterSystem, Entity, bool> IsAvailable;
    }

    public static class ActionsRegistry
    {
        public static readonly List<BotAction> ACTIONS = new List<BotAction>
        {
            new BotAction
            {
                Name = "stop",
                RequiredState = GameState.Any,
                Roles = BotRole.Any | BotRole.None,
                IsAvailable = (system, bot) => system.IsBusy(bot)
            },
            new BotAction
            {
                Name = "go_to",
                RequiredState = GameState.Franchise | GameState.Night | GameState.Day,
                Roles = BotRole.Any | BotRole.None,
                IsAvailable = (system, bot) => system.HasPlayers(bot)
            },
            new BotAction
            {
                Name = "select_card_left",
                RequiredState = GameState.Paused,
                Roles = BotRole.Any | BotRole.None,
                IsAvailable = (system, bot) => system.HasCardSelection(bot)
            },
            new BotAction
            {
                Name = "select_card_right",
                RequiredState = GameState.Paused,
                Roles = BotRole.Any | BotRole.None,
                IsAvailable = (system, bot) => system.HasCardSelection(bot)
            },
            new BotAction
            {
                Name = "role_chef",
                RequiredState = GameState.Franchise | GameState.Night | GameState.Day,
                Roles = BotRole.Waiter | BotRole.Dishwasher | BotRole.None,
                IsAvailable = (system, bot) => system.AlwaysAvailable(bot)
            },
            new BotAction
            {
                Name = "role_waiter",
                RequiredState = GameState.Franchise | GameState.Night | GameState.Day,
                Roles = BotRole.Chef | BotRole.Dishwasher | BotRole.None,
                IsAvailable = (system, bot) => system.AlwaysAvailable(bot)
            },
            new BotAction
            {
                Name = "start_game",
                RequiredState = GameState.Franchise,
                Roles = BotRole.Any | BotRole.None,
                IsAvailable = (system, bot) => system.CanMoveToStart(bot)
            },
            new BotAction
            {
                Name = "rename_restaurant",
                RequiredState = GameState.Night | GameState.Day,
                Roles = BotRole.Any | BotRole.None,
                IsAvailable = (system, bot) => system.AlwaysAvailable(bot)
            },
            new BotAction
            {
                Name = "service",
                RequiredState = GameState.Day | GameState.Franchise,
                Roles = BotRole.Waiter,
                IsAvailable = (system, bot) => system.HasWaitingGuests(bot)
            },
            new BotAction
            {
                Name = "extinguish_fires",
                RequiredState = GameState.Day | GameState.Franchise,
                Roles = BotRole.Chef,
                IsAvailable = (system, bot) => system.HasFires(bot)
            },
            new BotAction
            {
                Name = "clean_mess",
                RequiredState = GameState.Day,
                Roles = BotRole.Chef | BotRole.Waiter | BotRole.Dishwasher,
                IsAvailable = (system, bot) => system.HasMess(bot)
            },
            new BotAction
            {
                Name = "serve",
                RequiredState = GameState.Day | GameState.Franchise,
                Roles = BotRole.Waiter,
                IsAvailable = (system, bot) => system.HasServableFood(bot)
            },
            new BotAction
            {
                Name = "prepare_order",
                RequiredState = GameState.Day | GameState.Franchise,
                Roles = BotRole.Chef,
                IsAvailable = (system, bot) => system.HasUnsatisfiedOrders(bot)
            },
            new BotAction
            {
                Name = "prepare_dish",
                RequiredState = GameState.Day | GameState.Franchise,
                Roles = BotRole.Chef,
                IsAvailable = (system, bot) => system.AlwaysAvailable(bot)
            },
            new BotAction
            {
                Name = "empty_bin",
                RequiredState = GameState.Day,
                Roles = BotRole.Any,
                IsAvailable = (system, bot) => system.IsBinFull(bot)
            },
            new BotAction
            {
                Name = "wash_plates",
                RequiredState = GameState.Day | GameState.Franchise,
                Roles = BotRole.Chef | BotRole.Dishwasher,
                IsAvailable = (system, bot) => system.KitchenHasDirtyPlates(bot)
            },
            new BotAction
            {
                Name = "return_plates",
                RequiredState = GameState.Day | GameState.Franchise,
                Roles = BotRole.Waiter,
                IsAvailable = (system, bot) => system.HasDirtyPlates(bot)
            },

            // NYI: Check for Supplies, if present restock any empty dispensers

            // NYI: If we have dispensers and there are unstocked tables resupply them

            // NYI: If we have a hosting stand with waiting guests and an empty table seat them

            // NYI: If a floor buffer is present start buffing the floor

            // NYI: If we have a black flower and guests waiting for food use it

            // NYI: If we have a phone and free tables offer to call in customers

            // NYI: Throw burned things into the bin
        };
    }
}