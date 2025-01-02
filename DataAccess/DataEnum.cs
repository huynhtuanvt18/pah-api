using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public enum Status {
        Available = 1,
        Unavailable = 0,
        Unverified = 2
    }

    public enum Role {
        Buyer = 1,
        Seller = 2,
        Administrator = 3,
        Manager = 4,
        Staff = 5
    }

    public enum OrderStatus {
        Pending = 1,
        ReadyForPickup = 2,
        Delivering = 3,
        Delivered = 4,
        CancelApprovalPending = 10,
        CancelledBySeller = 12,
        CancelledByBuyer = 11,
        WaitingSellerConfirm = 5,
        Done = 6
    }

    public enum AddressType {
        Delivery = 1,
        Pickup = 2
    }

    public enum ProductType
    {
        ForSale = 1,
        Auction = 2,
    }

    public enum Condition
    {
        Mint = 1,
        NearMint = 2,
        VeryFine = 3,
        Good = 4,
        Poor = 5,
    }

    public enum AuctionStatus
    {
        Pending = 0,
        Unassigned = 1,
        Assigned = 2,
        Rejected = 3,
        RegistrationOpen = 4,
        Opened = 5,
        Ended = 6,
        EndedWithoutBids = 7,
        Unavailable = 8,
    }

    public enum SellerStatus
    {
        Pending = 1,
        Available = 2, 
        Unavailable = 0,
    }

    public enum BidStatus
    {        
        Active = 1,
        Retracted = 2,
        Register = 3,
        Refund = 4
    }

    public enum PaymentType {
        Wallet = 1,
        Zalopay = 2
    }

    public enum TransactionType {
        Deposit = 1,
        Withdraw = 2,
        Payment = 3,
        Refund = 4,
        DoneOrder = 5
    }

    public enum WithdrawalStatus {
        Pending = 1,
        Done = 2,
        Rejected = 3
    }
}
