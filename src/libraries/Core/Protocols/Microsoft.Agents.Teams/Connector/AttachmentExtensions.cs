﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Protocols.Primitives;
using Microsoft.Agents.Protocols.Serializer;
using Microsoft.Agents.Teams.Primitives;

namespace Microsoft.Agents.Teams.Connector
{
    /// <summary>
    /// Attachment extensions.
    /// </summary>
    public static class AttachmentExtensions
    {
        /// <summary>
        /// Converts normal attachment into the messaging extension attachment.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        /// <param name="previewAttachment">The preview attachment.</param>
        /// <returns>Messaging extension attachment.</returns>
        public static MessagingExtensionAttachment ToMessagingExtensionAttachment(this Attachment attachment, Attachment previewAttachment = null)
        {
            // We are recreating the attachment so that JsonSerializerSettings with ReferenceLoopHandling set to Error does not generate error
            // while serializing. Refer to issue - https://github.com/OfficeDev/BotBuilder-MicrosoftTeams/issues/52.
            return new MessagingExtensionAttachment
            {
                Content = attachment.Content,
                ContentType = attachment.ContentType,
                ContentUrl = attachment.ContentUrl,
                Name = attachment.Name,
                ThumbnailUrl = attachment.ThumbnailUrl,
                Preview = previewAttachment ?? ProtocolJsonSerializer.ToObject<Attachment>(ProtocolJsonSerializer.ToJson(attachment))
            };
        }
    }
}