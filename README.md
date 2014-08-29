trustfreemarket.com (formally known as Anark)
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

1. Update web.config with the connection string for your databases. (both debug and production)
2. Generate the database from /Models/SiteDB.edmx
2. Update SendEmail() in Globals.cs in the root folder with your SMTP credentials
3. Build and run. You may login with admin/admin

Questions or comments may be sent to admin@trustfreemarket.com
