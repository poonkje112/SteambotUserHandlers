using SteamKit2;
using System.Collections.Generic;
using SteamTrade;
using System;
using System.Timers;
using SteamTrade.TradeWebAPI;
using TradeAsset = SteamTrade.TradeOffer.TradeOffer.TradeStatusUser.TradeAsset;
using SteamTrade.TradeOffer;

namespace SteamBot
{
    public class KeyUserHandler : UserHandler
    {
        static string BotVersion = "2.4.0";

        //PRICE SETTINGS
        //PRICE IN SCRAPS
        //6.55
        static int SellPricePerKey = BuyPricePerKey + 4;// price in scrap, e.g. 31 / 9 = 3.55 ref
        //6.44
        static int BuyPricePerKey = 154; // price in scrap, e.g. 29 / 9 = 3.33 ref

        //SHOW PRICES SETTINGS
        static double show_sell_price = 17.55;
        static double show_buy_price = 17.11;

        static int TimerInterval = 170000;
        static int InviteTimerInterval = 2000;

        // NEW ------------------------------------------------------------------
        private readonly GenericInventory mySteamInventory;
        private readonly GenericInventory OtherSteamInventory;

        private bool tested;
        // ----------------------------------------------------------------------



        int UserMetalAdded, UserScrapAdded, UserRecAdded, UserRefAdded, UserKeysAdded, BotKeysAdded, BotMetalAdded, BotScrapAdded, BotRecAdded, BotRefAdded, InventoryMetal, InventoryScrap, InventoryRec, InventoryRef, InventoryKeys, OverpayNumKeys, ExcessInScrap, PreviousKeys, WhileLoop, InvalidItem = 0;

        double ExcessRefined = 0.0;

        bool InGroupChat, TimerEnabled, HasRun, HasErrorRun, ChooseDonate, AskOverpay, IsOverpaying, HasCounted = false;
        bool TimerDisabled = true;

        ulong uid;
        SteamID currentSID;
        SteamID ownerSID = 76561198075097366;

        Timer adTimer = new System.Timers.Timer(TimerInterval);
        Timer inviteMsgTimer = new System.Timers.Timer(InviteTimerInterval);

        public KeyUserHandler(Bot bot, SteamID sid)
            : base(bot, sid)
        {
            mySteamInventory = new GenericInventory(SteamWeb);
            OtherSteamInventory = new GenericInventory(SteamWeb);
        }


        public override void OnLoginCompleted()
        {
        }

        public override bool OnGroupAdd()
        {
            return false;
        }

        public override void OnTradeAwaitingConfirmation(long tradeOfferID)
        {
            Log.Warn("Trade ended awaiting confirmation");
            SendChatMessage("Please complete the confirmation to finish the trade");
        }

        public override void OnTradeSuccess()
        {
            Log.Success("Trade Complete.");
        }

        public override bool OnFriendAdd()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") added me!");
            // Using a timer here because the message will fail to send if you do it too quickly
            inviteMsgTimer.Interval = InviteTimerInterval;
            inviteMsgTimer.Elapsed += (sender, e) => OnInviteTimerElapsed(sender, e, EChatEntryType.ChatMsg);
            inviteMsgTimer.Enabled = true;
            return true;
        }

        public override void OnFriendRemove()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") removed me!");
        }

        public override void OnMessage(string message, EChatEntryType type)
        {
            Bot.log.Warn("Message in the chat: " + message);
            message = message.ToLower();

            if (message.Contains("hi") || message.Contains("hello") || message.Contains("sup") || message.Contains("hey"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Hello, to get started with trading type: trade or invite me to trade.");
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "To see a list of commands type: help");

                /** NEW */
                if (IsAdmin)
                {
                    //creating a new trade offer
                    var offer = Bot.NewTradeOffer(OtherSID);

                    //offer.Items.AddMyItem(0, 0, 0);
                    if (offer.Items.NewVersion)
                    {
                        string newOfferId;
                        if (offer.Send(out newOfferId))
                        {
                            Bot.AcceptAllMobileTradeConfirmations();
                            Log.Success("Trade offer sent : Offer ID " + newOfferId);
                        }
                    }

                    //creating a new trade offer with token
                    var offerWithToken = Bot.NewTradeOffer(OtherSID);

                    //offer.Items.AddMyItem(0, 0, 0);
                    if (offerWithToken.Items.NewVersion)
                    {
                        string newOfferId;
                        // "token" should be replaced with the actual token from the other user
                        if (offerWithToken.SendWithToken(out newOfferId, "token"))
                        {
                            Bot.AcceptAllMobileTradeConfirmations();
                            Log.Success("Trade offer sent : Offer ID " + newOfferId);
                        }
                    }
                }
            }

            //REGULAR chat commands

            if (message.Contains("trade"))
            {
                Bot.OpenTrade(OtherSID);
            }


            if (message.Contains("price"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "I am currently buying keys at " + show_buy_price + " Ref and selling at " + show_sell_price + " Ref.");
            }
            else if ((message.Contains("love") || message.Contains("luv") || message.Contains("<3")) && (message.Contains("y") || message.Contains("u")))
            {
                if (message.Contains("do"))
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, type, "I'd love you if I could, but robots are uncapable of love.");
                }
                else
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, type, "I'd love you if I could, but robots are uncapable of love.");
                }
            }
            else if (message.Contains("fuck") || message.Contains("suck") || message.Contains("dick") || message.Contains("cock") || message.Contains("tit") || message.Contains("boob") || message.Contains("pussy") || message.Contains("vagina") || message.Contains("cunt") || message.Contains("penis"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry, but as a robot I cannot perform sexual functions.");
            }
            else if (message == "donate")
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Please type that command into the TRADE WINDOW, not here! Thanks.");
            }
            else if (message == "owner")
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Here is a link to my owners profile http://steamcommunity.com/id/poonkje112/");
            }
            else if (message.Contains("help"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "To start trading type: trade");
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "If you want to see my current prices please type: price");
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Have you found a bug or ran into a problem and need hel please type: owner");
            }
            // ADMIN commands
            else if (IsAdmin)
            {
                if (message.StartsWith(".join"))
                {
                    // Usage: .join GroupID - e.g. ".join 103582791433582049" or ".join cts" - this will allow the bot to join a group's chatroom
                    if (message.Length >= 7)
                    {
                        if (message.Substring(6) == "cts")
                        {
                            uid = 103582791433582049;
                        }
                        else if (message.Substring(6) == "tf2")
                        {
                            uid = 103582791430075519;
                        }
                        else
                        {
                            ulong.TryParse(message.Substring(6), out uid);
                        }
                        var chatid = new SteamID(uid);
                        Bot.SteamFriends.JoinChat(chatid);
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Joining chat: " + chatid.ConvertToUInt64().ToString());
                        InGroupChat = true;
                        Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                        Bot.log.Success("Joining chat: " + chatid.ConvertToUInt64().ToString());
                    }
                }
                else if (message.StartsWith(".leave"))
                {
                    // Usage: .leave GroupID, same concept as joining
                    if (message.Length >= 8)
                    {
                        if (message.Substring(7) == "cts")
                        {
                            uid = 103582791433582049;
                        }
                        else if (message.Substring(7) == "tf2")
                        {
                            uid = 103582791430075519;
                        }
                        else
                        {
                            ulong.TryParse(message.Substring(7), out uid);
                        }
                        var chatid = new SteamID(uid);
                        Bot.SteamFriends.LeaveChat(chatid);
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Leaving chat: " + chatid.ConvertToUInt64().ToString());
                        InGroupChat = false;
                        Bot.log.Success("Leaving chat: " + chatid.ConvertToUInt64().ToString());
                    }
                }
                else if (message.StartsWith(".sell"))
                {
                    // Usage: .sell newprice "e.g. sell 26"
                    int NewSellPrice = 0;
                    if (message.Length >= 6)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Current selling price: " + SellPricePerKey + " scrap.");
                        int.TryParse(message.Substring(5), out NewSellPrice);
                        Bot.log.Success("Admin has requested that I set the new selling price from " + SellPricePerKey + " scrap to " + NewSellPrice + " scrap.");
                        SellPricePerKey = NewSellPrice;
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Setting new selling price to: " + SellPricePerKey + " scrap.");
                        Bot.log.Success("Successfully set new price.");
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I need more arguments. Current selling price: " + SellPricePerKey + " scrap.");
                    }
                }
                else if (message.StartsWith(".buy"))
                {
                    // Usage: .buy newprice "e.g. .buy 24"
                    int NewBuyPrice = 0;
                    if (message.Length >= 5)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Current buying price: " + BuyPricePerKey + " scrap.");
                        int.TryParse(message.Substring(4), out NewBuyPrice);
                        Bot.log.Success("Admin has requested that I set the new selling price from " + BuyPricePerKey + " scrap to " + NewBuyPrice + " scrap.");
                        BuyPricePerKey = NewBuyPrice;
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Setting new buying price to: " + BuyPricePerKey + " scrap.");
                        Bot.log.Success("Successfully set new price.");
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I need more arguments. Current buying price: " + BuyPricePerKey + " scrap.");
                    }
                }
                else if (message.StartsWith(".setsell"))
                {
                    // Usage: .sell newprice "e.g. sell 26"
                    double show_new_sell_price = 0;
                    if (message.Length >= 9)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Current selling price: " + show_sell_price + " ref.");
                        double.TryParse(message.Substring(8), out show_new_sell_price);
                        Bot.log.Success("Admin has requested that I set the new selling price from " + show_sell_price + " ref to " + show_new_sell_price + " ref.");
                        show_sell_price = show_new_sell_price;
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Setting new selling price to: " + show_sell_price + " ref.");
                        Bot.log.Success("Successfully set new price.");
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I need more arguments. Current selling price: " + SellPricePerKey + " scrap.");
                    }
                }
                else if (message.StartsWith(".setbuy"))
                {
                    // Usage: .buy newprice "e.g. .buy 24"
                    double show_new_buy_price = 0;
                    if (message.Length >= 8)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Current buying price: " + show_buy_price + "ref.");
                        double.TryParse(message.Substring(7), out show_new_buy_price);
                        Bot.log.Success("Admin has requested that I set the new selling price from " + show_buy_price + " ref to " + show_new_buy_price + "ref.");
                        show_buy_price = show_new_buy_price;
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Setting new buying price to: " + show_buy_price + " ref.");
                        Bot.log.Success("Successfully set new price.");
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I need more arguments. Current buying price: " + show_buy_price + " ref.");
                    }
                }
                else if (message.StartsWith(".gmessage"))
                {
                    // usage: say ".gmessage Hello!" to the bot will send "Hello!" into group chat
                    if (message.Length >= 10)
                    {
                        if (InGroupChat)
                        {
                            var chatid = new SteamID(uid);
                            string gmessage = message.Substring(10);
                            Bot.SteamFriends.SendChatRoomMessage(chatid, type, gmessage);
                            Bot.log.Success("Said into group chat: " + gmessage);
                        }
                        else
                        {
                            Bot.log.Warn("Cannot send message because I am not in a group chatroom!");
                        }
                    }
                }
                else if (message == ".advertise")
                {
                    // This will allow the bot to advertise into the last group it joined
                    if (InGroupChat)
                    {
                        if (TimerDisabled == true)
                        {
                            TimerEnabled = true;
                            Bot.SteamFriends.SendChatMessage(OtherSID, type, "Beginning advertisements.");
                            Bot.log.Success("Beginning advertisements.");
                            if (!HasRun)
                            {
                                adTimer.Interval = TimerInterval;
                                adTimer.Elapsed += (sender, e) => OnTimerElapsed(sender, e, type);
                                HasRun = true;
                            }
                        }
                        else if (TimerDisabled == false)
                        {
                            TimerEnabled = false;
                            Bot.SteamFriends.SendChatMessage(OtherSID, type, "Stopping advertisements.");
                            Bot.log.Warn("Stopping advertisements.");
                            HasRun = false;
                        }
                        Advertise(type);
                    }
                }
                else if (message == ".canceltrade")
                {
                    // Cancels the trade. Occasionally the message will be sent to YOU instead of the current user. Oops.
                    Trade.CancelTrade();
                    Bot.SteamFriends.SendChatMessage(currentSID, EChatEntryType.ChatMsg, "My creator has forcefully cancelled the trade. Whatever you were doing, he probably wants you to stop.");
                }
                else if (message == ".removeall")
                {
                    // Commenting this out because RemoveAllFriends is a custom function I wrote.
                    //Bot.SteamFriends.RemoveAllFriends();
                    //Bot.log.Warn("Removed all friends from my friends list.");
                }
            }
            else
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.ChatResponse);
            }
        }


        EChatEntryType type2;
        public override bool OnTradeRequest()
        {
            //Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") has requested to trade with me!");
            
            Bot.SteamFriends.SendChatMessage(OtherSID, type2, "I'm sending you a trade offer in 5 secconds!");
            System.Threading.Thread.Sleep(5000);
            Bot.OpenTrade(OtherSID);
            return false;
        }

        public override void OnTradeError(string error)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID,
                                              EChatEntryType.ChatMsg,
                                              "Error: " + error + "."
                                              );
            Bot.log.Warn(error);
            if (!HasErrorRun)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Oops something went wrong! Error: " + error);
                HasErrorRun = true;
            }
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
        }

        public override void OnTradeTimeout()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "Sorry, but you were either AFK or took too long and the trade was canceled.");
            Bot.log.Info("User was kicked because he was AFK.");
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
        }

        public override void OnTradeInit()
        {
            ReInit();
            TradeCountInventory(true);
            // NEW -------------------------------------------------------------------------------
            List<long> contextId = new List<long>();
            tested = false;

            /*************************************************************************************
             * 
             * SteamInventory AppId = 753 
             * 
             *  Context Id      Description
             *      1           Gifts (Games), must be public on steam profile in order to work.
             *      6           Trading Cards, Emoticons & Backgrounds. 
             *  
             ************************************************************************************/

            contextId.Add(1);
            contextId.Add(6);

            mySteamInventory.load(753, contextId, Bot.SteamClient.SteamID);
            OtherSteamInventory.load(753, contextId, OtherSID);

            if (!mySteamInventory.isLoaded | !OtherSteamInventory.isLoaded)
            {
                SendTradeMessage("Couldn't open an inventory, type 'errors' for more info.");
            }

            SendTradeMessage("Type 'test' to start.");
            // -----------------------------------------------------------------------------------

            if (InventoryKeys == 0)
            {
                Trade.SendMessage("I don't have any keys to sell right now! I am currently buying keys for " + show_buy_price + " ref.");
            }
            else if (InventoryMetal < BuyPricePerKey)
            {
                Trade.SendMessage("I don't have enough metal to buy keys! I am selling keys for " + show_sell_price + " ref.");
            }
            else
            {
                Trade.SendMessage("I am currently buying keys for " + show_buy_price + " ref, and selling keys for " + show_sell_price + " ref.");
            }
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            switch (inventoryItem.AppId)
            {
                case 440:
                    var item = Trade.CurrentSchema.GetItem(schemaItem.Defindex);

                    if (!HasCounted)
                    {
                        Trade.SendMessage("ERROR: I haven't finished counting my inventory yet! Please remove any items you added, and then re-add them or there could be errors.");
                    }
                    else if (InvalidItem >= 4)
                    {
                        Trade.CancelTrade();
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Please stop messing around. I am used for buying and selling keys only. I can only accept metal or keys as payment.");
                        Bot.log.Warn("Booted user for messing around.");
                        Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                    }
                    else if (item.Defindex == 5000)
                    {
                        // Put up scrap metal
                        UserMetalAdded++;
                        UserScrapAdded++;
                        Bot.log.Success("User added: " + item.ItemName);
                    }
                    else if (item.Defindex == 5001)
                    {
                        // Put up reclaimed metal
                        UserMetalAdded += 3;
                        UserRecAdded++;
                        Bot.log.Success("User added: " + item.ItemName);
                    }
                    else if (item.Defindex == 5002)
                    {
                        // Put up refined metal
                        UserMetalAdded += 9;
                        UserRefAdded++;
                        Bot.log.Success("User added: " + item.ItemName);
                    }
                    else if (item.Defindex == 5021)
                    {
                        // Put up keys
                        UserKeysAdded++;
                        Bot.log.Success("User added: " + item.ItemName);
                        // USER IS SELLING KEYS
                        if (!ChooseDonate)
                        {
                            // BOT ADDS METAL
                            int KeysToScrap = UserKeysAdded * BuyPricePerKey;
                            if (InventoryMetal < KeysToScrap)
                            {
                                Trade.SendMessage("I only have " + InventoryMetal + " scrap. You need to remove some keys.");
                                Bot.log.Warn("I don't have enough metal for the user.");
                            }
                            else
                            {
                                Trade.SendMessage("You have given me " + UserKeysAdded + " key(s). I will give you " + KeysToScrap + " scrap.");
                                Bot.log.Success("User gave me " + UserKeysAdded + " key(s). I will now give him " + KeysToScrap + " scrap.");
                                // Put up required metal
                                bool DoneAddingMetal = false;
                                while (!DoneAddingMetal)
                                {
                                    if (InventoryRef > 0 && BotMetalAdded + 9 <= KeysToScrap)
                                    {
                                        Trade.AddItemByDefindex(5002);
                                        Bot.log.Warn("I added Refined Metal.");
                                        BotMetalAdded += 9;
                                        BotRefAdded++;
                                        InventoryRef--;
                                    }
                                    else if (InventoryRec > 0 && BotMetalAdded + 3 <= KeysToScrap)
                                    {
                                        Trade.AddItemByDefindex(5001);
                                        Bot.log.Warn("I added Reclaimed Metal.");
                                        BotMetalAdded += 3;
                                        BotRecAdded++;
                                        InventoryRec--;
                                    }
                                    else if (InventoryScrap > 0 && BotMetalAdded + 1 <= KeysToScrap)
                                    {
                                        Trade.AddItemByDefindex(5000);
                                        Bot.log.Warn("I added Scrap Metal.");
                                        BotMetalAdded++;
                                        BotScrapAdded++;
                                        InventoryScrap--;
                                    }
                                    else if (InventoryScrap == 0 && BotMetalAdded + 2 == KeysToScrap)
                                    {
                                        Trade.SendMessage("Sorry, but I don't have enough metal to give you! Please remove some keys or add two keys.");
                                        Bot.log.Warn("Couldn't add enough metal for the user!");
                                        DoneAddingMetal = true;
                                    }
                                    else if (InventoryScrap == 0 && BotMetalAdded + 1 == KeysToScrap)
                                    {
                                        Trade.SendMessage("Sorry, but I don't have enough metal to give you! Please remove some keys or add a key.");
                                        Bot.log.Warn("Couldn't add enough metal for the user!");
                                        DoneAddingMetal = true;
                                    }
                                    else if (BotMetalAdded == KeysToScrap)
                                    {
                                        Trade.SendMessage("Added enough metal. " + BotRefAdded + " ref, " + BotRecAdded + " rec, " + BotScrapAdded + " scrap.");
                                        Bot.log.Success("Gave user enough metal!");
                                        DoneAddingMetal = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (item.Defindex == 5049 || item.Defindex == 5067 || item.Defindex == 5072 || item.Defindex == 5073 || item.Defindex == 5079 || item.Defindex == 5081 || item.Defindex == 5628 || item.Defindex == 5631 || item.Defindex == 5632)
                    {
                        Trade.SendMessage("I'm sorry but I can't accept " + item.ItemName + "! This key used to be a special key such as a winter key that has turned back into a normal one. Unfortunately the ID is different and I am not coded to handle them at the moment. Please remove it and exchange it for a normal key!");
                        Bot.log.Warn("User added a special key, but I cannot accept it.");
                    }
                    else
                    {
                        // Put up other items
                        Trade.SendMessage("Sorry, I don't accept " + item.ItemName + " Defindex: " + item.Defindex + "! I only accept metal/keys. Please remove it from the trade to continue.");
                        Bot.log.Warn("User added:  " + item.ItemName);
                        InvalidItem++;
                    }
                    // USER IS BUYING KEYS
                    if (!ChooseDonate)
                    {
                        if (UserMetalAdded % SellPricePerKey >= 0 && UserMetalAdded > 0)
                        {
                            // Count refined and convert to keys -- X scrap per key
                            int NumKeys = UserMetalAdded / SellPricePerKey;
                            if (NumKeys > 0 && NumKeys != PreviousKeys)
                            {
                                Trade.SendMessage("You put up enough metal for " + NumKeys + " key(s). Adding your keys now.");
                                Bot.log.Success("User put up enough metal for " + NumKeys + " key(s).");
                                if (NumKeys > InventoryKeys)
                                {
                                    double excess = ((NumKeys - BotKeysAdded) * SellPricePerKey) / 9.0;
                                    string refined = string.Format("{0:N2}", excess);
                                    Trade.SendMessage("I only have " + InventoryKeys + " in my backpack. :(");
                                    Bot.log.Warn("User wanted to buy " + NumKeys + " key(s), but I only have " + InventoryKeys + " key(s).");
                                    Trade.SendMessage("Please remove " + refined + " ref.");
                                    NumKeys = InventoryKeys;
                                }
                                // Add the keys to the trade window
                                for (int count = BotKeysAdded; count < NumKeys; count++)
                                {
                                    Trade.AddItemByDefindex(5021);
                                    Bot.log.Warn("I am adding Mann Co. Supply Crate Key.");
                                    BotKeysAdded++;

                                    //BUG FIX KEYS 1/2
                                    //InventoryKey--;
                                }
                                Trade.SendMessage("I have added " + BotKeysAdded + " key(s) for you.");
                                Bot.log.Success("I have added " + BotKeysAdded + " key(s) for the user.");
                                PreviousKeys = NumKeys;
                            }
                        }
                    }
                    break;
                case 753:
                    GenericInventory.ItemDescription tmpDescription = OtherSteamInventory.getDescription(inventoryItem.Id);
                    SendTradeMessage("Steam Inventory Item Added.");
                    SendTradeMessage("Type: {0}", tmpDescription.type);
                    SendTradeMessage("Marketable: {0}", (tmpDescription.marketable ? "Yes" : "No"));
                    break;

                default:
                    SendTradeMessage("Unknown item");
                    break;
            }
        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            var item = Trade.CurrentSchema.GetItem(schemaItem.Defindex);
            if (item.Defindex == 5000)
            {
                // Removed scrap metal
                UserMetalAdded--;
                UserScrapAdded--;
                Bot.log.Success("User removed: " + item.ItemName);
            }
            else if (item.Defindex == 5001)
            {
                // Removed reclaimed metal
                UserMetalAdded -= 3;
                UserRecAdded--;
                Bot.log.Success("User removed: " + item.ItemName);
            }
            else if (item.Defindex == 5002)
            {
                // Removed refined metal
                UserMetalAdded -= 9;
                UserRefAdded--;
                Bot.log.Success("User removed: " + item.ItemName);
            }
            else if (item.Defindex == 5021)
            {
                // Removed keys
                UserKeysAdded--;
                Bot.log.Success("User removed: " + item.ItemName);
            }
            else if (item.Defindex == 5049 || item.Defindex == 5067 || item.Defindex == 5072 || item.Defindex == 5073 || item.Defindex == 5079 || item.Defindex == 5081 || item.Defindex == 5628 || item.Defindex == 5631 || item.Defindex == 5632)
            {
                Bot.log.Warn("User removed special key.");
            }
            else
            {
                // Removed other items
                Bot.log.Warn("User removed: " + item.ItemName);
            }
            // User removes key from trade
            if (UserKeysAdded < (float)BotMetalAdded / BuyPricePerKey)
            {
                int KeysToScrap = UserKeysAdded * BuyPricePerKey;
                bool DoneAddingMetal = false;
                while (!DoneAddingMetal)
                {
                    WhileLoop++;
                    if (BotRefAdded > 0 && BotMetalAdded - 9 >= KeysToScrap)
                    {
                        Trade.RemoveItemByDefindex(5002);
                        Bot.log.Warn("I removed Refined Metal.");
                        BotMetalAdded -= 9;
                        BotRefAdded--;
                        InventoryRef++;
                    }
                    else if (BotRecAdded > 0 && BotMetalAdded - 3 >= KeysToScrap)
                    {
                        Trade.RemoveItemByDefindex(5001);
                        Bot.log.Warn("I removed Reclaimed Metal.");
                        BotMetalAdded -= 3;
                        BotRecAdded--;
                        InventoryRec++;
                    }
                    else if (BotScrapAdded > 0 && BotMetalAdded - 1 >= KeysToScrap)
                    {
                        Trade.RemoveItemByDefindex(5000);
                        Bot.log.Warn("I removed Scrap Metal.");
                        BotMetalAdded--;
                        BotScrapAdded--;
                        InventoryScrap++;
                    }
                    else if (BotMetalAdded == KeysToScrap)
                    {
                        DoneAddingMetal = true;
                    }
                    else if (WhileLoop > 50)
                    {
                        Trade.SendMessage("Error: I could not remove the proper amounts of metal from the trade. I might be out of scrap metal - try adding more keys if possible, or remove a few keys.");
                        WhileLoop = 0;
                        DoneAddingMetal = true;
                    }
                }
            }
            // User removes metal from trade
            while ((float)UserMetalAdded / SellPricePerKey < BotKeysAdded)
            {
                Trade.RemoveItemByDefindex(5021);
                Bot.log.Warn("I removed Mann Co. Supply Crate Key.");
                BotKeysAdded--;
                InventoryKeys++;
                PreviousKeys = BotKeysAdded;
                IsOverpaying = false;
            }
        }

        public override void OnTradeMessage(string message)
        {
            Bot.log.Info("[TRADE MESSAGE] " + message);
            message = message.ToLower();

            if (message == "buy")
            {
                Trade.SendMessage("This is an old command and is unnecessary. Simply add your metal and I will add keys for you when you've put up enough. I am currently selling keys for " + String.Format("{0:0.00}", (SellPricePerKey / 9.0)) + " ref each.");
                Bot.log.Warn("User tried to use an old command.");
            }

            if (message == "sell")
            {
                Trade.SendMessage("This is an old command and is unnecessary. Simply add your keys and I will automatically add the metal for you. I am currently buying keys for " + String.Format("{0:0.00}", (BuyPricePerKey / 9.0)) + " ref each.");
                Bot.log.Warn("User tried to use an old command.");
            }

            if (message == "donate")
            {
                ChooseDonate = true;
                Trade.SendMessage("Oh, you want to donate metal or keys? Thank you so much! Please put up your items and simply click \"Ready to Trade\" when done! If you want to buy or sell keys again you need to start a new trade with me.");
                Bot.log.Success("User wants to donate!");
            }

            if (message == "ready")
            {
                Trade.SendMessage("That is an old command and is unnecessary. I will automatically add keys and metal when you've put up enough.");
                Bot.log.Warn("User tried to use an old command.");
            }

            if (message == "continue")
            {
                if (AskOverpay)
                {
                    IsOverpaying = true;
                    Trade.SendMessage("You have chosen to continue overpaying. Click \"Ready to Trade\" again to complete the trade.");
                    Bot.log.Warn("User has chosen to continue overpaying!");
                }
                else
                {
                    Trade.SendMessage("You cannot use this command right now!");
                    Bot.log.Warn("User typed \"continue\" for no reason.");
                }
            }

        }

        public override void OnTradeReady(bool ready)
        {
            if (!ready)
            {
                Trade.SetReady(false);
            }
            else
            {
                Bot.log.Success("User is ready to trade!");
                if (Validate())
                {
                    Trade.SetReady(true);
                }
                else
                {
                    if (AskOverpay && OverpayNumKeys != 0 && !ChooseDonate)
                    {
                        double AdditionalRefined = (SellPricePerKey / 9.0) - ExcessRefined;
                        string addRef = string.Format("{0:N2}", AdditionalRefined);
                        string refined = string.Format("{0:N2}", ExcessRefined);
                        Trade.SendMessage("WARNING: You will be overpaying. If you'd like to continue, type \"continue\", otherwise remove " + refined + " ref, or add " + addRef + " ref. You cannot complete the trade unless you do so.");
                        Bot.log.Warn("User has added an excess of " + refined + " ref. He can add " + addRef + " ref for another key. Asking user if they want to continue.");
                    }
                    else
                    {
                        ResetTrade(false);
                    }
                }
            }
        }


        /* NEW */
        public override void OnNewTradeOffer(TradeOffer offer)
        {
            if (IsAdmin)
            {
                Log.Success("Recived an trade offer");
                string tradeid;
                if (offer.Accept(out tradeid))
                {
                    Log.Success("Recived an trade offer");
                    offer.Accept();
                    Bot.AcceptAllMobileTradeConfirmations();
                    Log.Success("Accepted trade offer successfully!");
                }
                else
                {
                    if (offer.Decline())
                    {
                        Log.Info("Declined trade offer : " + offer.TradeOfferId + " from untrusted user " + OtherSID.ConvertToUInt64());
                    }
                }
            }
        }

        private bool DummyValidation(List<TradeAsset> myAssets, List<TradeAsset> theirAssets)
        {
            //compare items etc
            if (myAssets.Count == theirAssets.Count)
            {
                return true;
            }
            return false;
        }

        public override void OnTradeAccept()
        {
            if (Validate() || IsAdmin)
            {
                bool success = Trade.AcceptTrade();
                if (success)
                {
                    if (!ChooseDonate)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Trying to accept mobile confirmation...");
                        System.Threading.Thread.Sleep(5000);
                        {
                            try
                            {
                                Bot.AcceptAllMobileTradeConfirmations();
                                Log.Success("Trade was successful!");

                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Trade was successful! ");
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "your the best!");
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "If you had a issue or concern please leave a comment on my profile!");
                                Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                            }
                            catch
                            {
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Something went wrong please contact my owner if this keeps happening!");
                            }
                        }
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Please accept the mobile trade confirmation! and type done when you are ready!");
                        Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                    }
                }
                else
                {
                    Log.Warn("Trade might have failed.");
                    Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                }
            }
            OnTradeClose();
        }

        public override void OnTradeClose()
        {
            bool didSomething = false;
            if (InventoryScrap < 9)
            {
                ulong[] smelt = new ulong[1];
                Bot.SetGamePlaying(440);
                Bot.GetInventory();
                Inventory myInventory = Bot.MyInventory;
                foreach (Inventory.Item item in myInventory.Items)
                {
                    if (item.Defindex == 5001)
                    {
                        smelt[0] = item.Id;
                        SteamBot.TF2GC.Crafting.CraftItems(Bot, smelt);
                        didSomething = true;
                        break;
                    }
                }
                if (!didSomething)
                {
                    foreach (Inventory.Item item in Trade.MyInventory.Items)
                    {
                        if (item.Defindex == 5002)
                        {
                            smelt[0] = item.Id;
                            SteamBot.TF2GC.Crafting.CraftItems(Bot, smelt);
                            didSomething = true;
                            break;
                        }
                    }
                }
            }
            Bot.SetGamePlaying(0);
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
            base.OnTradeClose();
        }

        public bool Validate()
        {
            int ScrapCount = 0;
            int KeyCount = 0;

            List<string> errors = new List<string>();

            foreach (TradeUserAssets id in Trade.OtherOfferedItems)
            {
                var item = Trade.OtherInventory.GetItem(id.assetid);
                if (item.Defindex == 5000)
                {
                    ScrapCount++;
                }
                else if (item.Defindex == 5001)
                {
                    ScrapCount += 3;
                }
                else if (item.Defindex == 5002)
                {
                    ScrapCount += 9;
                }
                else if (item.Defindex == 5021)
                {
                    KeyCount++;
                }
                else if (item.Defindex == 5049 || item.Defindex == 5067 || item.Defindex == 5072 || item.Defindex == 5073 || item.Defindex == 5079 || item.Defindex == 5081 || item.Defindex == 5628 || item.Defindex == 5631 || item.Defindex == 5632)
                {
                    var schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                    errors.Add("I'm sorry, but I cannot accept " + schemaItem.ItemName + " because it is not in my code to process keys that used to be special keys like scorched keys or winter keys. Please exchange your key for a normal one and try again!");
                }
                else
                {
                    var schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                    errors.Add("I can't accept " + schemaItem.ItemName + "!");
                }
            }

            if (ChooseDonate)
            {
                foreach (TradeUserAssets id in Trade.OtherOfferedItems)
                {
                    var item = Trade.OtherInventory.GetItem(id.assetid);
                    if (item.Defindex == 5021 || item.Defindex == 5049 || item.Defindex == 5067 || item.Defindex == 5072 || item.Defindex == 5073 || item.Defindex == 5079 || item.Defindex == 5081 || item.Defindex == 5628 || item.Defindex == 5631 || item.Defindex == 5632 || item.Defindex == 5000 || item.Defindex == 5001 || item.Defindex == 5002) { }
                    else
                    {
                        var schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                        errors.Add("I'm sorry, but I cannot accept " + schemaItem.ItemName + "!");
                    }
                }

                if (BotMetalAdded > 0 || BotKeysAdded > 0)
                {
                    errors.Add("You can't do that :( I still have items put up!");
                }
            }
            else if (UserKeysAdded > 0)
            {
                Bot.log.Warn("User has " + KeyCount + " key(s) put up. Verifying if " + (float)BotMetalAdded / BuyPricePerKey + " == " + KeyCount + ".");
                if (KeyCount != (float)BotMetalAdded / BuyPricePerKey)
                {
                    errors.Add("Something went wrong. Either you do not have the correct amount of keys or I don't have the correct amount of metal.");
                }
            }
            else if (UserMetalAdded % SellPricePerKey != 0 && !IsOverpaying)
            {
                // Count refined and convert to keys -- X scrap per key
                OverpayNumKeys = UserMetalAdded / SellPricePerKey;
                ExcessInScrap = UserMetalAdded - (OverpayNumKeys * SellPricePerKey);
                ExcessRefined = (ExcessInScrap / 9.0);
                string refined = string.Format("{0:N2}", ExcessRefined);
                Trade.SendMessage("You put up enough metal for " + OverpayNumKeys + " key(s), with " + refined + " ref extra.");
                Bot.log.Success("User put up enough metal for " + OverpayNumKeys + " key(s), with " + refined + " ref extra.");
                if (OverpayNumKeys == 0)
                {
                    double AdditionalRefined = (SellPricePerKey / 9.0) - ExcessRefined;
                    string addRef = string.Format("{0:N2}", AdditionalRefined);
                    errors.Add("ERROR: You need to add " + addRef + " ref for a key.");
                    Bot.log.Warn("User doesn't have enough metal added, and needs add " + addRef + " ref for a key.");
                }
                else if (OverpayNumKeys >= 1)
                {
                    errors.Add("You have put up more metal than what I'm asking.");
                    AskOverpay = true;
                }
            }
            else if (UserMetalAdded > 0 && !IsOverpaying)
            {
                if (ScrapCount < BotKeysAdded * SellPricePerKey || (ScrapCount > BotKeysAdded * SellPricePerKey))
                {
                    errors.Add("You must put up exactly " + String.Format("{0:0.00}", (SellPricePerKey / 9.0)) + " ref per key.");
                }
            }

            // send the errors
            if (errors.Count != 0)
                Trade.SendMessage("There were errors in your trade: ");

            foreach (string error in errors)
            {
                Trade.SendMessage(error);
            }

            return errors.Count == 0;
        }


        bool hasDuplicateID(List<ulong> list)
        {
            var hashset = new HashSet<ulong>();
            foreach (var id in list)
            {
                if (!hashset.Add(id))
                {
                    return true;
                }
            }
            return false;
        }


        public void TradeCountInventory(bool message)
        {
            // Let's count our inventory

            Inventory.Item[] inventory = Trade.MyInventory.Items;
            InventoryMetal = 0;
            InventoryKeys = 0;
            InventoryRef = 0;
            InventoryRec = 0;
            InventoryScrap = 0;
            Bot.log.Warn("Items on BP: " + inventory.Length);
            foreach (Inventory.Item item in inventory)
            {
                if (item.Defindex == 5000)
                {
                    InventoryMetal++;
                    InventoryScrap++;
                }
                else if (item.Defindex == 5001)
                {
                    InventoryMetal += 3;
                    InventoryRec++;
                }
                else if (item.Defindex == 5002)
                {
                    InventoryMetal += 9;
                    InventoryRef++;
                }
                else if (item.Defindex == 5021)
                {
                    InventoryKeys++;
                }
            }
            if (message)
            {
                double MetalToRef = (InventoryMetal / 9.0) - 0.01;
                string refined = string.Format("{0:N2}", MetalToRef);
                Trade.SendMessage("Current stock: ");
                Trade.SendMessage(InventoryKeys + " keys");
                Trade.SendMessage(InventoryRef + " ref");
                Trade.SendMessage(InventoryRec + " rec");
                Trade.SendMessage(InventoryScrap + " scrap");
                //Trade.SendMessage("Current stock: I have " + refined + " ref (" + InventoryRef + " ref, " + InventoryRec + " rec, and " + InventoryScrap + " scrap) and " + InventoryKeys + " key(s) in my backpack.");
                Bot.log.Success("Current stock: I have " + refined + " ref (" + InventoryRef + " ref, " + InventoryRec + " rec, and " + InventoryScrap + " scrap) and " + InventoryKeys + " key(s) in my backpack.");
            }
            HasCounted = true;
        }

        public void ReInit()
        {
            UserMetalAdded = 0;
            UserRefAdded = 0;
            UserRecAdded = 0;
            UserScrapAdded = 0;
            UserKeysAdded = 0;
            BotKeysAdded = 0;
            BotMetalAdded = 0;
            BotRefAdded = 0;
            BotRecAdded = 0;
            BotScrapAdded = 0;
            OverpayNumKeys = 0;
            PreviousKeys = 0;
            ExcessInScrap = 0;
            ExcessRefined = 0.0;
            WhileLoop = 0;
            InvalidItem = 0;
            HasErrorRun = false;
            ChooseDonate = false;
            AskOverpay = false;
            IsOverpaying = false;
            HasCounted = false;
            currentSID = OtherSID;
            Bot.log.Info("All reseted");
        }

        public void Advertise(EChatEntryType type)
        {
            if (TimerEnabled == true)
            {
                adTimer.Enabled = true;
                TimerDisabled = false;

            }
            if (TimerEnabled == false)
            {
                adTimer.Enabled = false;
                adTimer.Stop();
                TimerDisabled = true;
            }
        }

        private void OnTimerElapsed(object source, ElapsedEventArgs e, EChatEntryType type)
        {
            var chatid = new SteamID(uid);
            string adMessage;
            if (InventoryMetal > BuyPricePerKey && InventoryKeys > 0)
            {
                adMessage = "I am a keybanking trade bot! I am buying keys for " + show_buy_price + " ref and selling keys for " + show_sell_price + " ref. Send me a trade!";
            }
            else if (InventoryMetal < BuyPricePerKey)
            {
                adMessage = "I am a keybanking trade bot! I am only selling keys for " + show_sell_price + " ref at the moment. Send me a trade!";
            }
            else if (InventoryKeys == 0)
            {
                adMessage = "I am a keybanking trade bot! I am only buying keys for " + show_buy_price + " ref at the moment. Send me a trade!";
            }
            else
            {
                adMessage = "I am a keybanking trade bot! I am buying keys for " + show_buy_price + " ref and selling keys for " + show_sell_price + " ref. Send me a trade!";
            }
            Bot.SteamFriends.SendChatRoomMessage(chatid, type, adMessage);
            Bot.log.Success("Advertised into group chat: " + adMessage);
        }

        private void OnInviteTimerElapsed(object source, ElapsedEventArgs e, EChatEntryType type)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Hi. You have added Keybanking bot! Just trade me, and add your keys or metal to begin! ");
            Bot.log.Success("Sent welcome message.");
            inviteMsgTimer.Enabled = false;
            inviteMsgTimer.Stop();
        }

        public void ResetTrade(bool message)
        {

            for (int count = 0; count < BotKeysAdded; count++)
            {
                Trade.RemoveAllItemsByDefindex(5021);
            }
            for (int count = 0; count < BotScrapAdded; count++)
            {
                Trade.RemoveItemByDefindex(5000);
            }
            for (int count = 0; count < BotRecAdded; count++)
            {
                Trade.RemoveItemByDefindex(5001);
            }
            for (int count = 0; count < BotRefAdded; count++)
            {
                Trade.RemoveItemByDefindex(5002);
            }
            BotKeysAdded = 0;
            BotMetalAdded = 0;
            BotRefAdded = 0;
            BotRecAdded = 0;
            BotScrapAdded = 0;
            ChooseDonate = false;
            TradeCountInventory(message);
            Trade.SendMessage("Something went wrong! Scroll up to read the errors.");
            Bot.log.Warn("Something went wrong! I am resetting the trade.");
            Trade.SendMessage("I have reset the trade. Please try again. (If you chose to donate, you will need to type \"donate\" again)");
            Bot.log.Success("Reset trade.");
        }
    }
}
