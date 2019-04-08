﻿using System;
using System.Collections;

namespace Emitter.Utility
{
    #region ReverseTrie

    /// <summary>
    /// Represents a trie with a reverse-pattern search.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReverseTrie
    {
        private readonly Hashtable Children;
        private readonly short Level = 0;
        private MessageHandler Value = default(MessageHandler);

        /// <summary>
        /// Constructs a node of the trie.
        /// </summary>
        /// <param name="level">The level of this node within the trie.</param>
        public ReverseTrie(short level)
        {
            this.Level = level;
            this.Children = new Hashtable();
        }

        /// <summary>
        /// Adds a new handler for the channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        public void RegisterHandler(string channel, MessageHandler value)
        {
            // Add the value or replace it.
            this.AddOrUpdate(CreateKey(channel), 0, () => value, (old) => value);
        }

        /// <summary>
        /// Unregister the handler from the trie.
        /// </summary>
        /// <param name="channel"></param>
        public void UnregisterHandler(string channel)
        {
            MessageHandler removed;
            this.TryRemove(CreateKey(channel), 0, out removed);
        }

        /// <summary>
        /// Retrieves a set of values.
        /// </summary>
        /// <param name="query">The query to retrieve.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public IEnumerable Match(string channel)
        {
            // Matches
            var result = new ArrayList();

            // Get the query
            var query = CreateKey(channel);

            // Get the matching stack
            var matches = new Stack();

            // Push the root
            object childNode;
            matches.Push(this);
            while (matches.Count != 0)
            {
                var current = matches.Pop() as ReverseTrie;
                if (current.Value != default(object))
                    result.Add(current.Value);

                var level = current.Level + 1;
                if (level >= query.Length)
                    break;

                if (Utils.TryGetValueFromHashtable(current.Children, "+", out childNode))
                    matches.Push(childNode);
                if (Utils.TryGetValueFromHashtable(current.Children, query[level], out childNode))
                    matches.Push(childNode);
            }

            return result;
        }

        #region Private Members

        /// <summary>
        /// Creates a query for the trie from the channel name.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static string[] CreateKey(string channel)
        {
            return channel.Split('/');
        }

        /// <summary>
        /// Adds or updates a specific value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private object AddOrUpdate(string[] key, int position, AddFunc addFunc, UpdateFunc updateFunc)
        {
            if (position >= key.Length)
            {
                lock (this)
                {
                    // There's already a value
                    if (this.Value != default(object))
                        return updateFunc(this.Value);

                    // No value, add it
                    this.Value = addFunc();
                    return this.Value;
                }
            }

            // Create a child
            var child = Utils.GetOrAddToHashtable(Children, key[position], new ReverseTrie((short)position)) as ReverseTrie;
            return child.AddOrUpdate(key, position + 1, addFunc, updateFunc);
        }

        /// <summary>
        /// Attempts to remove a specific key from the Trie.
        /// </summary>
        private bool TryRemove(string[] key, int position, out MessageHandler value)
        {
            if (position >= key.Length)
            {
                lock (this)
                {
                    // There's no value
                    value = this.Value;
                    if (this.Value == default(MessageHandler))
                        return false;

                    this.Value = default(MessageHandler);
                    return true;
                }
            }

            // Remove from the child
            object child;
            if (Utils.TryGetValueFromHashtable(Children, key[position], out child))
                return ((ReverseTrie)child).TryRemove(key, position + 1, out value);

            value = default(MessageHandler);
            return false;
        }

        #endregion Private Members
    }

    #endregion ReverseTrie
}
