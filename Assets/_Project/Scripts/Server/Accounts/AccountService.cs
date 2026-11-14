// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.Collections.Generic;
using CyberMU.Server.Persistence;
using CyberMU.Shared.Network;
using Mirror;
using UnityEngine;

namespace CyberMU.Server.Accounts
{
    // Limites de nome de conta e senha, vindos do Inspector do authenticator.
    // DECISÃO TÉCNICA PROVISÓRIA: limites do MU original PENDENTES DE VERIFICAÇÃO.
    public struct AccountRules
    {
        public int AccountNameMinLength;
        public int AccountNameMaxLength;
        public int PasswordMinLength;
        public int PasswordMaxLength;
    }

    public struct LoginResult
    {
        public LoginResultCode Code;
        public string Message;   // texto mostrado ao jogador
        public int AccountId;    // só preenchido quando aceito
        public bool Accepted => Code == LoginResultCode.Accepted;
    }

    // Regras do login no servidor: valida conta/senha, cria conta nova,
    // confere senha e controla quais contas estão conectadas agora.
    // Escreve no Console: conta criada / login aceito / login recusado.
    // NUNCA escreve a senha.
    public static class AccountService
    {
        // Conta conectada agora -> conexão dela (e o caminho inverso).
        private static readonly Dictionary<int, NetworkConnectionToClient> onlineByAccount = new Dictionary<int, NetworkConnectionToClient>();
        private static readonly Dictionary<NetworkConnectionToClient, int> onlineByConnection = new Dictionary<NetworkConnectionToClient, int>();

        public static int OnlineCount => onlineByAccount.Count;

        // Tenta fazer login. Se aceito, a conta JÁ entra na lista de conectadas
        // (quem chama deve chamar MarkOffline se algo falhar depois disso).
        public static LoginResult TryLogin(NetworkConnectionToClient conn, string accountName, string password, AccountRules rules)
        {
            accountName = accountName ?? string.Empty;
            password = password ?? string.Empty;

            if (!IsValidText(accountName, rules.AccountNameMinLength, rules.AccountNameMaxLength))
            {
                // O nome inválido não é impresso (pode conter qualquer coisa).
                return Refuse(conn, "(nome inválido)", LoginResultCode.InvalidAccountName,
                    $"Conta inválida: use de {rules.AccountNameMinLength} a {rules.AccountNameMaxLength} letras ou números.");
            }
            if (!IsValidText(password, rules.PasswordMinLength, rules.PasswordMaxLength))
            {
                return Refuse(conn, accountName, LoginResultCode.InvalidPassword,
                    $"Senha inválida: use de {rules.PasswordMinLength} a {rules.PasswordMaxLength} letras ou números.");
            }
            if (!ServerDatabase.IsOpen)
            {
                return Refuse(conn, accountName, LoginResultCode.ServerError, "Servidor sem banco de dados. Tente mais tarde.");
            }

            try
            {
                AccountRecord account = AccountRepository.FindByName(accountName);

                if (account == null)
                {
                    // Conta não existe: cria (decisão do projeto: criação automática no primeiro login).
                    byte[] salt = PasswordHasher.CreateSalt();
                    int iterations = PasswordHasher.DefaultIterations;
                    byte[] hash = PasswordHasher.ComputeHash(password, salt, iterations);
                    int newId = AccountRepository.Create(accountName, Convert.ToBase64String(hash), Convert.ToBase64String(salt), iterations);
                    Debug.Log($"[Accounts] Conta criada: '{accountName}' (Id {newId}) | conexão {conn.connectionId}");
                    return Accept(conn, newId, accountName);
                }

                bool passwordOk = PasswordHasher.Verify(password,
                    Convert.FromBase64String(account.Salt), account.HashIterations,
                    Convert.FromBase64String(account.PasswordHash));
                if (!passwordOk)
                    return Refuse(conn, account.Name, LoginResultCode.WrongPassword, "Senha incorreta");

                if (onlineByAccount.ContainsKey(account.Id))
                    return Refuse(conn, account.Name, LoginResultCode.AlreadyConnected, "Conta já conectada");

                AccountRepository.UpdateLastLogin(account.Id);
                return Accept(conn, account.Id, account.Name);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Accounts] ERRO no login da conta '{accountName}': {e.GetType().Name}: {e.Message}");
                return Refuse(conn, accountName, LoginResultCode.ServerError, "Erro no servidor. Tente mais tarde.");
            }
        }

        // Tira a conta desta conexão da lista de conectadas (se estiver).
        // Seguro chamar várias vezes e para conexões que nunca logaram.
        public static void MarkOffline(NetworkConnectionToClient conn)
        {
            if (conn == null) return;
            if (onlineByConnection.TryGetValue(conn, out int accountId))
            {
                onlineByConnection.Remove(conn);
                onlineByAccount.Remove(accountId);
                Debug.Log($"[Accounts] Conta Id {accountId} saiu (conexão {conn.connectionId}). Conectadas agora: {OnlineCount}");
            }
        }

        // Esvazia a lista (servidor iniciando ou parando).
        public static void ClearOnline()
        {
            onlineByAccount.Clear();
            onlineByConnection.Clear();
        }

        private static LoginResult Accept(NetworkConnectionToClient conn, int accountId, string accountName)
        {
            onlineByAccount[accountId] = conn;
            onlineByConnection[conn] = accountId;
            Debug.Log($"[Accounts] Login aceito: '{accountName}' (Id {accountId}) | conexão {conn.connectionId} | conectadas agora: {OnlineCount}");
            return new LoginResult { Code = LoginResultCode.Accepted, Message = "Login aceito", AccountId = accountId };
        }

        private static LoginResult Refuse(NetworkConnectionToClient conn, string accountNameForLog, LoginResultCode code, string message)
        {
            Debug.Log($"[Accounts] Login recusado: '{accountNameForLog}' | conexão {conn.connectionId} | motivo: {message}");
            return new LoginResult { Code = code, Message = message, AccountId = 0 };
        }

        // Só letras e números do alfabeto básico (A-Z, a-z, 0-9), sem acentos.
        private static bool IsValidText(string text, int min, int max)
        {
            if (text.Length < min || text.Length > max) return false;
            foreach (char c in text)
            {
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
                if (!ok) return false;
            }
            return true;
        }
    }
}
#endif
