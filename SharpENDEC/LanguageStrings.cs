using System;

namespace SharpENDEC
{
    public static class LanguageStrings
    {
        public static string RecoveredFromFailure(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Le chien de garde a récemment détecté un problème et a effacé toutes les données XML stockées.\r\n" +
                           "Les alertes précédemment relayées peuvent être relayées à nouveau lorsque le prochain battement de coeur arrive.";
                case "en":
                default:
                    return "The watchdog has recently detected a problem, and has cleared all XML data stored.\r\n" +
                           "Alerts previously relayed may relay again when the next heartbeat arrives.";
            }
        }

        public static string WatchdogForcedRestartWarning(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Cela fait plus de 5 minutes depuis le dernier battement de coeur. Si aucun battement de coeur n'est détecté dans les 5 minutes supplémentaires, le programme redémarrera automatiquement.";
                case "en":
                default:
                    return "It has been more than 5 minutes since the last heartbeat. If a heartbeat is not detected within 5 additional minutes, the program will automatically restart.";
            }
        }

        public static string WatchdogForceRestartingProcess(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Le battement de coeur a été présumé mort. Redémarrage de tous les services dans quelques instants.";
                case "en":
                default:
                    return "The heartbeat has been presumed dead. Restarting all services in a few moments.";
            }
        }

        public static string WatchingFiles(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Surveillance du répertoire pour les alertes.";
                case "en":
                default:
                    return "Watching directory for alerts.";
            }
        }

        public static string HeartbeatDetected(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Battement de coeur détecté.";
                case "en":
                default:
                    return "Heartbeat detected.";
            }
        }

        public static string AlertDetected(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Alerte détectée.";
                case "en":
                default:
                    return "Alert detected.";
            }
        }

        public static string ConnectedToServer(string lang, string host, int port)
        {
            switch (lang)
            {
                case "fr":
                    return $"Connecté à {host} sur le port {port}.";
                case "en":
                default:
                    return $"Connected to {host} on port {port}.";
            }
        }

        public static string HostTimedOut(string lang, string host)
        {
            switch (lang)
            {
                case "fr":
                    return $"{host} n'a envoyé aucune donnée dans le délai minimum.";
                case "en":
                default:
                    return $"{host} hasn't sent any data within the minimum time limit.";
            }
        }

        public static string RestartingAfterException(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Le fil de capture est mort de façon inattendue. Il redémarrera automatiquement dans quelques instants.";
                case "en":
                default:
                    return "The capture thread has died unexpectedly. It will automatically restart in a few moments.";
            }
        }

        public static string FileDownloaded(string lang, string directory, string filename, string host)
        {
            switch (lang)
            {
                case "fr":
                    return $"Fichier enregistré : {directory}\\{filename} | De : {host}";
                case "en":
                default:
                    return $"File saved: {directory}\\{filename} | From: {host}";
            }
        }

        public static string DownloadingFiles(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Téléchargement de fichiers à partir du signal cardiaque reçu.";
                case "en":
                default:
                    return "Downloading files from received heartbeat.";
            }
        }

        public static string FileIgnoredDueToMatchingPair(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Paire correspondante détectée. Ce fichier ne sera pas traité davantage.";
                case "en":
                default:
                    return "Matching pair detected. This file won't be processed.";
            }
        }

        public static string FilesIgnoredDueToMatchingPairs(string lang, int count)
        {
            switch (lang)
            {
                case "fr":
                    return $"{count} fichier(s) avaient des paires correspondantes et n'ont pas été traités.";
                case "en":
                default:
                    return $"{count} file(s) had matching pairs, and were not processed.";
            }
        }

        public static string FileIgnoredDueToPreferences(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Les préférences ne permettent pas le relais de cette alerte. Ce fichier ne sera pas traité davantage.";
                case "en":
                default:
                    return "Preferences do not allow the relay of this alert. This file won't be processed.";
            }
        }

        public static string GeneratingProductText(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Génération de texte en cours.";
                case "en":
                default:
                    return "Generating text.";
            }
        }

        public static string GeneratingProductAudio(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Génération audio en cours.";
                case "en":
                default:
                    return "Generating audio.";
            }
        }

        public static string GeneratedProductEmpty(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Il n’y avait rien à générer.";
                case "en":
                default:
                    return "There was nothing to generate.";
            }
        }

        public static string PlayingAudio(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "La lecture audio a commencé.";
                case "en":
                default:
                    return "Audio playback started.";
            }
        }

        public static string CapturedFromFileWatcher(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "File Watcher a capturé un fichier :";
                case "en":
                default:
                    return "File Watcher captured a file:";
            }
        }

        public static string ProcessingStream(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Flux de données de traitement.";
                case "en":
                default:
                    return "Processing data stream.";
            }
        }

        public static string ProcessedStream(string lang, int total, DateTime started)
        {
            switch (lang)
            {
                case "fr":
                    return $"Traité {total} dans {(DateTime.Now - started).TotalSeconds}s.";
                case "en":
                default:
                    return $"Processed {total} in {(DateTime.Now - started).TotalSeconds}s.";
            }
        }

        public static string LastDataReceived(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Dernières données reçues :";
                case "en":
                default:
                    return "Last data received:";
            }
        }

        public static string ThreadShutdown(string lang, string name)
        {
            switch (lang)
            {
                case "fr":
                    return $"{name} a été arrêté.";
                case "en":
                default:
                    return $"{name} has been shutdown.";
            }
        }

        public static string StartingConnection(string lang, string server, int port)
        {
            switch (lang)
            {
                case "fr":
                    return $"Démarrage de la connexion à {server}:{port}.";
                case "en":
                default:
                    return $"Starting connection to {server}:{port}.";
            }
        }

        public static string ElevationSecurityProblem(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return $"L’exécution de SharpENDEC elevated n’améliore pas les performances et peut poser un risque de sécurité dans certaines situations.\r\n" +
                        $"Veuillez exécuter SharpENDEC sans élévation la prochaine fois que vous l’exécuterez !";
                case "en":
                default:
                    return $"Running SharpENDEC elevated doesn't improve performance, and may pose a security risk in some situations.\r\n" +
                        $"Please run SharpENDEC without elevation next time you run it!";
            }
        }

        internal static string ConfigurationLossProblem(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return $"Vous perdrez probablement votre configuration parce que vous utilisez un compte invité." +
                        $"S’il vous plaît exécuter SharpENDEC sous un utilisateur normal pour garder votre configuration !";
                case "en":
                default:
                    return $"You will most likely lose your configuration because you are using a guest account.\r\n" +
                        $"Please run SharpENDEC under a normal user to keep your configuration!";
            }
        }
    }
}
