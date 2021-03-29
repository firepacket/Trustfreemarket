﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnarkRE
{
    public enum EscrowState
    {
        Created = 0,
        Accepted = 1,
        Closed = 2,
        Arbitrating = 3
    }

    public enum EscrowClosedBy : int
    {
        Buyer = 0,
        Seller = 1,
        Autorelease = 2,
        ArbitForBuyer = 3,
        ArbitForSeller = 4,
        SellerReject = 5,
        BuyerCancel = 6,
        BuyerRelease = 7,
        SellerNoship = 8,
        SellerRelease = 9,
        SellerCancel = 10
    }

    public enum ListingAdditionType
    {
        Shipping = 0,
        Description = 1,
        SingleSelect = 2,
        MultiSelect = 3
    }
}