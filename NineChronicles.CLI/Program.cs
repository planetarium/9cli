using Libplanet;
using Libplanet.Crypto;

namespace NineChronicles.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        var pk = await Address.GetKey();
        if (await Graphql.GetNextTxNonce(pk.ToAddress()) == 0)
        {
            Console.WriteLine("Account activation required. Please input activation code");
            var activationCode = Console.ReadLine();
            string? txId = await Address.ActivateAccount(pk, activationCode!);
            if (txId is null)
            {
                return;
            }

            string txResult = await Graphql.WaitTxMining(txId);
            if (txResult == "SUCCESS")
            {
                Console.WriteLine("Account activated.");
            }
            else
            {
                Console.WriteLine("Account activation failed. Please try with another activation code");
            }
        }

        var avatarAddress = await Avatar.SelectAvatar(pk);
        if (avatarAddress is null)
        {
            Console.WriteLine("No avatar selected. Exiting...");
            return;
        }

        await DoFaucetCurrency(pk);

        Console.WriteLine("Your agent status:");
        var _ = GameAction.GetStatus(pk);
    }

    static async Task DoFaucetCurrency(PrivateKey pk)
    {
        Console.WriteLine("You can add NCG, Crystal to your address. Use faucet? [y/N]");
        var useFaucet = Console.ReadLine();
        bool faucet = false;
        bool valid = false;
        while (!valid)
        {
            if (useFaucet is "" or null)
            {
                useFaucet = "";
                valid = true;
            }

            if (!new[] { "y", "Y", "n", "N", "" }.Contains(useFaucet))
            {
                Console.Write("Select only Y or N [y/N]: ");
                useFaucet = Console.ReadLine();
            }
            else
            {
                valid = true;
                if (new[] { "y", "Y" }.Contains(useFaucet))
                {
                    faucet = true;
                    break;
                }
            }
        }

        if (faucet)
        {
            Console.WriteLine("We'll give 10K NCG and 1M Crystal to your address");
            var faucetResult = await Address.FaucetCurrency(pk);
        }
    }
}
