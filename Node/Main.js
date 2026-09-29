let express = require('express')
let app = express();

app.get('/abuout' , function(req , res){res.send('about data');});

app.listen(3000, function(){console.log('listening on port 3000');});