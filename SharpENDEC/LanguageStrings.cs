using System;

namespace SharpENDEC
{
    public static class LanguageStrings
    {
        public static string RecoveredFromFailure(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Le chien de garde a récemment détecté un problème et a effacé toutes les données XML stockées.\r\n" +
                           "Les alertes précédemment relayées peuvent être relayées à nouveau lorsque le prochain battement de coeur arrive.";
                case "inuktitut":
                    return "ᓇᐅᑦᑎᖅᓱᖅᑎ ᖃᐅᔨᓵᓚᐅᖅᑐᖅ ᐊᑲᐅᙱᓕᐅᕈᑎᒥᒃ, ᐲᔭᐃᓯᒪᓪᓗᓂᓗ ᑕᒪᐃᓐᓂᒃ XML ᑲᑎᖅᓱᖅᓯᒪᔪᓂ ᑐᖅᑯᖅᑕᐅᓯᒪᓪᓗᑎᒃ.\r\n" +
                           "ᑭᖑᓂᑦᑎᓐᓂ ᐅᖃᐅᓯᐅᖃᑦᑕᖅᓯᒪᔪᑦ ᖃᐅᔨᑎᑦᑎᔾᔪᑏᑦ ᐅᖃᐅᓯᐅᒃᑲᓐᓂᓕᕈᓐᓇᖅᐳᑦ ᑭᖑᓪᓕᕐᒥ ᐆᒻᒪᓯᕆᔪᖃᒃᑲᓐᓂᓕᖅᐸᑦ.";
                case "english":
                default:
                    return "The watchdog has recently detected a problem, and has cleared all XML data stored.\r\n" +
                           "Alerts previously relayed may relay again when the next heartbeat arrives.";
            }
        }

        public static string WatchdogForcedRestartWarning(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Cela fait plus de 5 minutes depuis le dernier battement de coeur. Si aucun battement de coeur n'est détecté dans les 5 minutes supplémentaires, le programme redémarrera automatiquement.";
                case "inuktitut":
                    return "5 ᒥᓂᑦ ᐅᖓᑖᓃᓕᖅᑐᖅ ᐆᒻᒪᑎᖓ ᑭᖑᓪᓕᖅᐸᐅᓚᐅᖅᑎᓪᓗᒍ. ᐆᒻᒪᓯᕆᔪᖅ ᖃᐅᔨᔭᐅᙱᑉᐸᑦ 5 ᒥᓂᑦᒥᑦ, ᐱᓕᕆᐊᖅ ᐱᒋᐊᑲᐅᑎᒋᓂᐊᖅᑐᖅ.";
                case "english":
                default:
                    return "It has been more than 5 minutes since the last heartbeat. If a heartbeat is not detected within 5 additional minutes, the program will automatically restart.";
            }
        }

        public static string WatchdogForceRestartingProcess(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Le battement de coeur a été présumé mort. Redémarrage de tous les services dans quelques instants.";
                case "english":
                default:
                    return "The heartbeat has been presumed dead. Restarting all services in a few moments.";
            }
        }

        //public static string WatchingFiles(string lang)
        //{
        //    switch (lang)
        //    {
        //        case "french":
        //            return "Surveillance du répertoire pour les alertes.";
        //        case "english":
        //        default:
        //            return "Watching directory for alerts.";
        //    }
        //}

        public static string HeartbeatDetected(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Battement de coeur détecté.";
                case "inuktitut":
                    return "ᐆᒻᒪᑎᒥᒃ ᖃᐅᔨᔭᐅᔪᖅ.";
                case "english":
                default:
                    return "Heartbeat detected.";
            }
        }

        public static string AlertQueued(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Alerte ajoutée à la file d’attente.";
                case "inuktitut":
                    return "ᐊᓘᑦ ᐃᓚᔭᐅᔪᖅ.";
                case "english":
                default:
                    return "Alert added to the queue.";
            }
        }

        public static string ConnectedToServer(string lang, string host, int port)
        {
            switch (lang)
            {
                case "french":
                    return $"Connecté à {host} sur le port {port}.";
                case "inuktitut":
                    return $"ᐊᑕᔪᖅ {host} ᑐᓚᒃᑕᕐᕕᖕᒥ {port}.";
                case "english":
                default:
                    return $"Connected to {host} on port {port}.";
            }
        }

        public static string HostTimedOut(string lang, string host)
        {
            switch (lang)
            {
                case "french":
                    return $"{host} n'a envoyé aucune donnée dans le délai minimum.";
                case "english":
                default:
                    return $"{host} hasn't sent any data within the minimum time limit.";
            }
        }

        public static string RestartingAfterException(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Le fil de capture est mort de façon inattendue. Il redémarrera automatiquement dans quelques instants.";
                case "english":
                default:
                    return "The capture thread has died unexpectedly. It will automatically restart in a few moments.";
            }
        }

        public static string FileDownloaded(string lang, string host)
        {
            switch (lang)
            {
                case "french":
                    return $"Données enregistrées. | De : {host}";
                case "inuktitut":
                    return $"ᑎᑎᕋᖅᓯᒪᔪᑦ ᖃᐅᔨᓴᖅᑕᐅᓂᑯᐃᑦ ᑐᖅᑯᖅᑕᐅᓯᒪᔪᑦ. | ᐅᕙᙵᑦ: {host}";
                case "english":
                default:
                    return $"Data saved. | From: {host}";
            }
        }

        public static string DownloadingFiles(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Téléchargement de fichiers à partir du signal cardiaque reçu.";
                case "inuktitut":
                    return "ᖃᕆᑕᐅᔭᒃᑯᑦ ᐱᓗᒋᑦ ᑎᑎᖅᑲᑦ ᐱᔭᐅᔪᓂᒃ ᐆᒻᒪᑎᐅᑉ ᕐᑳᖓᓂ.";
                case "english":
                default:
                    return "Downloading files from received heartbeat.";
            }
        }

        public static string FileIgnoredDueToMatchingPair(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Paire correspondante détectée. Les données ne seront pas traitées.";
                case "inuktitut":
                    return "ᐊᔾᔨᒌᓂᒃ ᐃᓚᒌᖕᓂᒃ ᖃᐅᔨᔪᖃᓚᐅᖅᑐᖅ. ᑎᑎᕋᖅᓯᒪᔪᑦ ᖃᐅᔨᓴᖅᑕᐅᓂᑯᐃᑦ ᐱᓕᕆᐊᖑᔾᔮᖏᑦᑐᑦ.";
                case "english":
                default:
                    return "Matching pair detected. The data won't be processed.";
            }
        }

        public static string DataIgnoredDueToMatchingPairs(string lang, int count)
        {
            switch (lang)
            {
                case "french":
                    return $"{count} chaîne(s) avaient des paires correspondantes et n'ont pas été traités.";
                case "inuktitut":
                    return $"{count} ᐊᒃᖢᓇᐅᔭᑦ ᐊᔾᔨᒌᓂᒃ ᐱᖃᑎᒌᓚᐅᖅᐳᑦ, ᐱᓕᕆᐊᖑᓚᐅᙱᖦᖢᑎᒡᓗ.";
                case "english":
                default:
                    return $"{count} string(s) had matching pairs, and were not processed.";
            }
        }

        public static string AlertInvalid(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Cette alerte n'est pas valide et ne peut pas être traitée.";
                case "english":
                default:
                    return "This alert is invalid and cannot be processed.";
            }
        }

        public static string AlertIgnoredDueToPreferences(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Cette alerte ne sera pas traitée en raison des préférences de l'utilisateur.";
                case "english":
                default:
                    return "This alert won't be processed due to the user preferences.";
            }
        }

        public static string GeneratingProductText(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Génération de texte en cours.";
                case "inuktitut":
                    return "ᐊᐅᓚᔪᑦ ᑎᑎᖅᑲᐃᑦ.";
                case "english":
                default:
                    return "Generating text.";
            }
        }

        public static string GeneratingProductAudio(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Génération audio en cours.";
                case "inuktitut":
                    return "ᓂᐱᖃᐅᑎᓕᐅᕐᓗᓂ.";
                case "english":
                default:
                    return "Generating audio.";
            }
        }

        public static string GeneratedProductEmpty(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Il n’y avait rien à générer.";
                case "inuktitut":
                    return "ᐱᑕᖃᓚᐅᖏᑦᑐᖅ ᐊᐅᓚᔾᔭᒋᐊᕐᓂᕐᒧᑦ.";
                case "english":
                default:
                    return "There was nothing to generate.";
            }
        }

        public static string PlayingAudio(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "La lecture audio a commencé.";
                case "inuktitut":
                    return "ᓂᐱᖃᐅᑎᒃᑯᑦ ᐱᙳᐊᕐᓂᖅ ᐱᒋᐊᓚᐅᖅᑐᖅ.";
                case "english":
                default:
                    return "Audio playback started.";
            }
        }

        public static string CapturedFromFileWatcher(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "De nouvelles données ont été saisies.";
                case "inuktitut":
                    return "ᓄᑖᑦ ᓈᓴᐅᑏᑦ/ᑎᑎᖅᑲᐃᑦ ᐱᔭᐅᓚᐅᖅᑐᑦ.";
                case "english":
                default:
                    return "New data was captured.";
            }
        }

        public static string ProcessingStream(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Flux de données de traitement.";
                case "inuktitut":
                    return "ᐱᓕᕆᐊᖃᕐᓂᖅ ᓈᓴᐅᑎᓂᒃ/ᑎᑎᖅᑲᓂᒃ.";
                case "english":
                default:
                    return "Processing data stream.";
            }
        }

        public static string ProcessedStream(string lang, int total, DateTime started)
        {
            switch (lang)
            {
                case "french":
                    return $"Traité {total} dans {(DateTime.Now - started).TotalSeconds}s.";
                case "inuktitut":
                    return $"ᐱᓕᕆᐊᖑᓪᓗᓂ {total} {(DateTime.Now - started).TotalSeconds}s.";
                case "english":
                default:
                    return $"Processed {total} in {(DateTime.Now - started).TotalSeconds}s.";
            }
        }

        public static string LastDataReceived(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Dernières données reçues :";
                case "inuktitut":
                    return "ᑭᖑᓪᓕᖅᐹᒥ ᑎᑎᕋᖅᓯᒪᔪᑦ ᐱᔭᐅᓚᐅᖅᑐᑦ:";
                case "english":
                default:
                    return "Last data received:";
            }
        }

        public static string ThreadShutdown(string lang, string name)
        {
            switch (lang)
            {
                case "french":
                    return $"{name} a été arrêté.";
                case "inuktitut":
                    return "ᑭᖑᓪᓕᖅᐹᒥ ᑎᑎᕋᖅᓯᒪᔪᑦ ᐱᔭᐅᓚᐅᖅᑐᑦ:";
                case "english":
                default:
                    return $"{name} was stopped.";
            }
        }

        public static string StartingConnection(string lang, string server, int port)
        {
            switch (lang)
            {
                case "french":
                    return $"Démarrage de la connexion à {server}:{port}.";
                case "inuktitut":
                    return $"ᐱᒋᐊᕐᓗᓂ ᑲᓱᖅᓯᒪᓂᕐᒥᑦ {server}:{port}.";
                case "english":
                default:
                    return $"Starting connection to {server}:{port}.";
            }
        }

        public static string ElevationSecurityProblem(string lang)
        {
            switch (lang)
            {
                case "french":
                    return $"L’exécution de SharpENDEC elevated n’améliore pas les performances et peut poser un risque de sécurité dans certaines situations.\r\n" +
                        $"Veuillez exécuter SharpENDEC sans élévation la prochaine fois que vous l’exécuterez !";
                case "english":
                default:
                    return $"Running SharpENDEC elevated doesn't improve performance, and may pose a security risk in some situations.\r\n" +
                        $"Please run SharpENDEC without elevation next time you run it!";
            }
        }

        public static string ConfigurationLossProblem(string lang)
        {
            switch (lang)
            {
                case "french":
                    return $"Vous perdrez probablement votre configuration parce que vous utilisez un compte invité.\r\n" +
                        $"S’il vous plaît exécuter SharpENDEC sous un utilisateur normal pour garder votre configuration !";
                case "english":
                default:
                    return $"You will lose your configuration because you are using a guest account.\r\n" +
                        $"Please run SharpENDEC under a normal user to keep your configuration!";
            }
        }

        public static string DataPreviouslyProcessed(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Les données ont été ignorées car elles se trouvaient déjà dans la file d’attente ou dans l’historique.";
                case "english":
                default:
                    return "The data was skipped because it's already in either the queue or history.";
            }
        }

        public static string GenericProcessingValueOfValue(string lang, int x, int y)
        {
            switch (lang)
            {
                case "french":
                    return $"Traitement {x} de {y}.";
                case "english":
                default:
                    return $"Processing {x} of {y}.";
            }
        }

        public static string DownloadFailure(string lang)
        {
            // Failed to download the file.
            switch (lang)
            {
                case "french":
                    return $"Impossible de télécharger le fichier.";
                case "english":
                default:
                    return $"Failed to download the file.";
            }
        }

        public static string AlertIgnoredDueToBlacklist(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Cette alerte ne sera pas traitée en raison de la liste noire.";
                case "english":
                default:
                    return "This alert won't be processed further due to the blacklist.";
            }
        }

        public static string AlertIgnoredDueToExpiry(string lang)
        {
            switch (lang)
            {
                case "french":
                    return "Cette alerte ne sera plus traitée parce qu'elle a expiré.";
                case "english":
                default:
                    return "This alert won't be processed further because it has expired.";
            }
        }
    }
}
