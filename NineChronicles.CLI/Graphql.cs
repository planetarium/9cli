using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Libplanet;
using Libplanet.Crypto;
using NineChronicles.CLI.Type;

namespace NineChronicles.CLI;

public static class Graphql
{
    public static GraphQLHttpClient Client = new(
        // "http://9c-main-rpc-1.nine-chronicles.com/graphql",
        "http://localhost:31280/graphql",
        new SystemTextJsonSerializer()
    );

    /// <summary>
    /// The general function to request any query and get anything.
    /// You have to parse query result on your own.
    /// </summary>
    /// <param name="query">The complete, valid GQL query string to request</param>
    /// <returns>Data part of query result if query succeeded or return error..</returns>
    public static async Task<(bool success, T data, GraphQLError[]? errors)> Query<T>(string query)
    {
        var resp = await Client.SendQueryAsync<T>(new GraphQLRequest(query));
        // var response = ToObject<T>(resp.Data);
        return (resp.Errors is null, resp.Data, resp.Errors);
    }

    public static async Task<(bool, ActionTxQueryResponseType, GraphQLError[]?)> Action(PrivateKey pk, string queryBody)
    {
        var nonce = await GetNextTxNonce(pk.ToAddress());
        var actionTxQuery = $@"query {{ actionTxQuery(
            publicKey: ""{pk.PublicKey}"", nonce: {nonce}
        ) {{ {queryBody} }} }}";
        return await Query<ActionTxQueryResponseType>(actionTxQuery);
    }

    public static async Task<(bool, string)> Stage(byte[] tx, byte[] signature)
    {
        var signTxQuery = $@"query {{
            transaction {{
                signTransaction(
                    unsignedTransaction: ""{ByteUtil.Hex(tx)}"",
                    signature: ""{ByteUtil.Hex(signature)}""
                )
            }}
        }}";
        (bool success, TransactionResponseType data, GraphQLError[]? errors) =
            await Query<TransactionResponseType>(signTxQuery);
        var stageQuery = $@"mutation {{
            stageTransaction(payload: ""{data.Transaction.SignTransaction}"")
        }}";
        (success, MutationResponseType result, errors) = await Query<MutationResponseType>(stageQuery);
        return (success, result.stageTransaction);
    }

    public static async Task<string> WaitTxMining(string txId)
    {
        Console.WriteLine("Waiting for block mining...");
        string txResult = await TxResult(txId);
        while (txResult == "STAGING")
        {
            Thread.Sleep(1000);
            txResult = await TxResult(txId);
        }

        return txResult;
    }

    public static async Task<long> GetNextTxNonce(Libplanet.Address address)
    {
        var query = $@"query {{
            transaction {{
                nextTxNonce(address: ""{address}"")
            }}
        }}";
        (bool success, TransactionResponseType data, GraphQLError[]? errors) =
            await Query<TransactionResponseType>(query);
        return data.Transaction.NextTxNonce;
    }

    public static async Task<string> TxResult(string txId)
    {
        var query = $@"query {{
            transaction {{
                transactionResult(txId: ""{txId}"") {{
                    txStatus
                }}
            }}
        }}";
        (bool success, TransactionResponseType data, GraphQLError[]? errors) =
            await Query<TransactionResponseType>(query);
        return data.Transaction.TransactionResult.TxStatus;
    }
}
