Set identity_insert customers on
INSERT INTO [dbo].[Customers]
		([CustomerId], [CustomerName],[Email],[Phone],[Address],[City],[ZipCode],[LastUpdate],[Comment])
     VALUES
		('1','John Smith','john.smith@somewhere.com','','','NewYork','','2019-02-27T13:45:02','customer updated & verified'),
('2','Sara Konor','sara.konor@terminator.com','0123456','Top street','Dallas','123 45','2020-12-22T17:40:15','Top street
Dallas123 45'),
('3','Anders Andersson','anders.andersson@here.se','34561','','NewYork','556 67','2019-02-27T13:45:02','<root><message>abc &amps; d</message></root>')
