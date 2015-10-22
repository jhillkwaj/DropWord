using System;

namespace Unibill.Impl
{
    public interface IReceiptStore
    {
        bool hasItemReceiptForFilebundle(string id);
        string getItemReceiptForFilebundle(string id);
        PurchasableItem getItemFromFileBundle(string id);
    }
}
