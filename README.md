# nft-sale-demo-api

After you run the api for the first time, the sql tables will be created via Entity Framework migration. 

Insert the following for example nfts to work with in the demo.
```
INSERT INTO public."Nfts" (id, name, rarity, base_cost, quantity, step_cost, step_quantity, image_url) VALUES (1, 'Sword of Doom', 'Common', 10000000, 100, 5, 10, 'QmNaGTLK9rq9muBXLK1yMptcdspXFarJ8QSLBwCjuvArqA');
INSERT INTO public."Nfts" (id, name, rarity, base_cost, quantity, step_cost, step_quantity, image_url) VALUES (2, 'Sword of Doom', 'Uncommon', 25000000, 75, 10, 10, 'QmRhU3sqMvcetBFyxywLF5en2NBduhoTSjEYvAD4MDnCJe');
INSERT INTO public."Nfts" (id, name, rarity, base_cost, quantity, step_cost, step_quantity, image_url) VALUES (3, 'Sword of Doom', 'Rare', 50000000, 50, 25, 10, 'QmQnGEvU18oNm926r6MCSSLcysKUpSudWP6bXU5PHCchCy');
INSERT INTO public."Nfts" (id, name, rarity, base_cost, quantity, step_cost, step_quantity, image_url) VALUES (4, 'Sword of Doom', 'Legendary', 200000000, 25, 100, 5, 'QmVuuNVCg7CbzLN3LZb7iZhVH5zA53ZeJbffrdgWJxnmuB');
INSERT INTO public."Nfts" (id, name, rarity, base_cost, quantity, step_cost, step_quantity, image_url) VALUES (5, 'Sword of Doom', 'Mythic', 500000000, 5, 250, 1, 'QmPm9wuKbeBTEQEU12878zKvyK5zwN1VZWe63csLCG91Qg');
INSERT INTO public."Nfts" (id, name, rarity, base_cost, quantity, step_cost, step_quantity, image_url) VALUES (6, 'Sword of Doom', 'Common', 10000000, 100, 5, 10, 'QmNaGTLK9rq9muBXLK1yMptcdspXFarJ8QSLBwCjuvArqA');
```
