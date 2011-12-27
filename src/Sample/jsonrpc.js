JsonRpc = function(url) {
	this._url = url;
	this._id = 0;
};

JsonRpc.prototype = {
	call: function (/* ... */) {
		var args = Array.prototype.slice.call(arguments),
		    req = {
		    	jsonrpc: '2.0',
		    	id: ++this._id,
		    	method: args.shift(),
		    	params: this._shiftUntilFunction(args)
		    },
			callback = args.shift() || function () { },
			scope = args.shift() || this;

		this._doJsonPost(this._url, req, function(success, data) {
			if(!success) {
				callback.call(scope, undefined, false, data);
			} else if(data.error) {
				callback.call(scope, undefined, false, data.error.message);
			} else {
				callback.call(scope, data.result, true);
			}
		});
	},
	
	_shiftUntilFunction : function (args) {
		var ret = [];
		while (args[0] && Object.prototype.toString.apply(args[0]) !== '[object Function]') {
			ret.push(args.shift());
		}
		return ret;
	},
	
	_doJsonPost: function (url, data, callback) {
		var xhr = new XMLHttpRequest();
		xhr.open("POST", url, true);
		xhr.setRequestHeader('Content-Type', 'application/json');
		xhr.onreadystatechange = function () {
			if (xhr.readyState !== 4) {
				return;
			}

			var contentType = xhr.getResponseHeader('Content-Type');

			if (xhr.status !== 200) {
				callback(false, 'Expected HTTP response "200 OK", found "' + xhr.status + ' ' + xhr.statusText + '"');
			} else if (contentType.indexOf('application/json') !== 0) {
				callback(false, 'Expected JSON encoded response, found "' + contentType + '"');
			} else {
				callback(true, JSON.parse(this.responseText));
			}
		};
		xhr.send(JSON.stringify(data));
	}
};