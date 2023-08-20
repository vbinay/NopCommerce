insert into MessageTemplate(Name,BccEmailAddresses,Subject,Body,IsActive,DelayBeforeSend,DelayPeriodId,AttachedDownloadId,EmailAccountId,LimitedToStores,IsMaster,MasterId,EmailTypeId,IsToKitchenPrinter)
values('Customer.NewCustomerCreatedNotification',null,'New customer in %Store.Name% Created.','<p><a href="%Store.URL%"> %Store.Name%</a> <br /><br />Hello %Customer.FullName% account has been created with email id %Customer.Email% in the <a href="%Store.URL%">%Store.Name%</a>&nbsp;. Kindly review it for activation, <br /><br /><br /><br /></p>',1,null,0,0,1,0,0,0,1,0)
