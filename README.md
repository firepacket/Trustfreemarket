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
3. Update SendEmail() in Globals.cs in the root folder with your SMTP credentials
4. Add all the missing references from the packages folder. It may be tedeous to do by hand but you only have to do it once!
5. Build and run. You may login with admin/admin
6. You have to add your own Categories in the Categories table. It doesn't happen automatically

Questions or comments may be sent to admin@trustfree.market


-------

P.S. It's 2021 and there's still no decent bitcoin marketplace. We plan on changing that.
We will also be correcting issues in this code that prevented many of you from being able to run this yourselves. You will find the instructions will now work, you just have to add all the references in the packages directory.

We have some new features including an encrypted chat between buyer and seller with strong client side encryption (AES 256 CBC /w RIPEMD160 Authentication (new IV each msg))
Arbitration built into the GUI allows for release of the encryptionn chat key to us but NOT your escrow keys (Totally seperate and optional!) This will all be open-sourced!

Websockets and slowpolling, so you can execute your trades faster in chat and feel confident nobody can see your address or personal details.
Userprofiles and a focus on the feedback system are top priorities.

We also plan on allowing you to spend your coins directly from escrow on the site without having to sweep the key like a paperwallet (if you choose to pay the miner fee).

## We have now cryptographically signed all CSS, HTML, and Javascript, sent from the server using RSA/SHA512 public/private key signatures, and we have a cross-browser userscript client-side plugin that can validate all the code you are running is authentic. It will notify you if the check fails and where it failed. This was the #1 request for this application and it's working in beta right now! This comes as an additional protection to SSL.

Keep checking back to see how we've implemented these features and let us know of any concerns!
