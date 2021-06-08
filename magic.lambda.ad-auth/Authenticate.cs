﻿/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.ad_auth
{
    /// <summary>
    /// [ad-auth.authenticate] slot that authenticates a user over Active Directory.
    /// </summary>
    [Slot(Name = "ad-auth.authenticate")]
    public class Authenticate : ISlot
    {
        readonly IConfiguration _configuration;

        public Authenticate(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Slot implementation.
        /// </summary>
        /// <param name="signaler">Signaler that raised signal.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Retrieving LDAP configuration.
            var path = _configuration["magic:ldap:path"];

            // Retrieving username from arguments.
            var username = input.Children.FirstOrDefault(x => x.Name == "username")?.GetEx<string>() ??
                throw new ArgumentException("No [username] provided to [ad-auth.authentication]");

            // Retrieving password from arguments.
            var password = input.Children.FirstOrDefault(x => x.Name == "password")?.GetEx<string>() ??
                throw new ArgumentException("No [password] provided to [ad-auth.authentication]");

            using (var user = new DirectoryEntry(path, username, password))
            {
                if (user.NativeObject == null)
                    throw new ArgumentException($"Access denied");
            }

            // To keep API similar to existing authenticate slots, we make sure we set node's value/result/return-value to true.
            input.Value = true;
        }
    }
}