https://trustfree.market (formally known as Anark)
========

Webbased secure 3-party escrow platform

Uses multipart keys to allow a buyer and seller to complete a bitcoin transaction with a mediator (the site)
while preventing any party from having the keys needed to spend the coins from escrow. 

There is no hotwallet.

The code borrows heavily from bitescrow.org and the BitcoinAddressUtility and is compatible with third party
implementations. The escrow part of the site is handled through a WebAPI controller making single-page clients
and offline spending possible.

The rest of the site runs ASP MVC 4 using Entity Framework and SimpleMembership for authentication. 
Blockchain.info is used for btc balance checks. For price data, Bitpay is used along with bitcoincharts as a backup.

Other features include thumbnails, featured listings, moderation features, and a crude feedback system

STEPS TO SET UP

1. Update web.config with the connection string for your databases. (both debug (update: no more debug db) and production)
2. Generate the database from /Models/SiteDB.edmx
2. Update SendEmail() in Globals.cs in the root folder with your SMTP credentials
3. Build and run. You may login with admin/admin

Questions or comments may be sent to admin@trustfree.market


P.S. It's 2021 and there's still no decent bitcoin marketplace. We plan on changing that.
We will also be correcting issues in this code that prevented many of you from being able to run this yourselves.

We have some new features including an encrypted chat between buyer and seller with strong client side encryption (AES 256 CBC /w RIPEMD160 Authentication (new IV each msg))
Arbitration built into the GUI allows for release of the encryptionn chat key to us but NOT your escrow keys (Totally seperate and optional!)

Websockets and slowpolling, so you can execute your trades faster in chat and feel confident nobody can see your address or personal details.
Userprofiles and a focus on the feedback system are top priorities.

We also plan on allowing you to spend your coins directly from escrow on the site without having to sweep the key like a paperwallet (if you choose to pay the miner fee).

Keep checking back to see how we've implemented these features and let us know of any concerns!
