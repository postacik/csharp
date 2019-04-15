﻿using System;
using System.Collections.Generic;
using System.Text;
using Emitter.Messages;
using Emitter.Utility;

namespace Emitter
{
    public partial class Connection
    {
        #region Subscribe

        /// <summary>
        /// Asynchronously subscribes to a particular channel of emitter.io service. Uses the default
        /// key that should be specified in the constructor.
        /// </summary>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <param name="handler">The callback to be invoked every time the message is received.</param>
        /// <returns>The message identifier for this operation.</returns>
        [Obsolete("The On method is obsolete, please use the Subscribe method instead.")]
        public ushort On(string channel, MessageHandler handler)
        {
            return this.Subscribe(channel, handler);
        }

        /// <summary>
        /// Asynchronously subscribes to a particular channel of emitter.io service.
        /// </summary>
        /// <param name="key">The key to use for this subscription request.</param>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <param name="handler">The callback to be invoked every time the message is received.</param>
        /// <returns>The message identifier for this operation.</returns>
        [Obsolete("The On method is obsolete, please use the Subscribe method instead.")]
        public ushort On(string key, string channel, MessageHandler handler)
        {
            return this.Subscribe(key, channel, handler);
        }

        /// <summary>
        /// Asynchronously subscribes to a particular channel of emitter.io service.
        /// </summary>
        /// <param name="key">The key to use for this subscription request.</param>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <param name="handler">The callback to be invoked every time the message is received.</param>
        /// <param name="last">The last x messages to request when we subcribe.</param>
        /// <returns>The message identifier for this operation.</returns>
        [Obsolete("The On method is obsolete, please use the Subscribe method with the WithLast option instead.")]
        public ushort On(string key, string channel, MessageHandler handler, int last)
        {
            // Register the handler
            this.Trie.RegisterHandler(channel, handler);

            // Subscribe
            return this.Client.Subscribe(new string[] { FormatChannel(key, channel, Options.WithLast(last)) }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        }

        public ushort Subscribe(string channel, MessageHandler handler, params string[] options)
        {
            if (this.DefaultKey == null)
                throw EmitterException.NoDefaultKey;
            return this.Subscribe(this.DefaultKey, channel, handler, options);
        }

        public ushort Subscribe(string key, string channel, MessageHandler handler, params string[] options)
        {
            // Register the handler
            this.Trie.RegisterHandler(channel, handler);

            // Subscribe
            return this.Client.Subscribe(new string[] { FormatChannel(key, channel, options) }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        }
        #endregion Subscribe

        #region Unsubscribe
        /// <summary>
        /// Asynchonously unsubscribes from a particular channel of emitter.io service. Uses the default
        /// key that should be specified in the constructor.
        /// </summary>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <returns>The message identifier for this operation.</returns>
        public ushort Unsubscribe(string channel)
        {
            if (this.DefaultKey == null)
                throw EmitterException.NoDefaultKey;
            return this.Unsubscribe(this.DefaultKey, channel);
        }

        /// <summary>
        /// Asynchonously unsubscribes from a particular channel of emitter.io service.
        /// </summary>
        /// <param name="key">The key to use for this unsubscription request.</param>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <returns>The message identifier for this operation.</returns>
        public ushort Unsubscribe(string key, string channel)
        {
            // Unregister the handler
            this.Trie.UnregisterHandler(key);

            // Unsubscribe
            return this.Client.Unsubscribe(new string[] { FormatChannel(key, channel) });
        }

        #endregion Unsubscribe
    }
}
