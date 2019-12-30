(function ($, React, ReactDOM, i18n, axios) {
    function getUrlParameter(name) {
        name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
        var regex = new RegExp('[\\?&]' + name + '=([^&#]*)','i');
        var results = regex.exec(location.search);
        return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
    };
    
    function number_format(n) {
        n += "";
        var arr = n.split(".");
        var re = /(\d{1,3})(?=(\d{3})+$)/g;
        return arr[0].replace(re, "$1,") + (arr.length == 2 ? "." + arr[1] : "");
    }

    class CustInfo extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};
        }

        render() {

            return (
                <div>
                    ZZZZZ
                </div>                
            )
        }
    }
    
    class CustInfoPanel extends React.Component {
        _Tabs=["Customer Info","Financial Log","Deposit Info","Transfer Info","Withdrawal Info","Login Log"];

        constructor(props) {
            super(props);
            this.state = { CustInfoData: null, TabIdx: 0 };
        }

        onTab(idx) {
            this.setState({TabIdx: idx});
        }

        render() {
            let dataContainer = null;
            let TabIdx = this.state.TabIdx;
            if(TabIdx == 0){
                dataContainer = <CustInfo CustInfoData={this.state.CustInfoData}/>
            } else {

            }

            var tabBtn = this._Tabs.map(function(item, index){
                return <li className="nav-item" key={index}>
                <a className={"nav-link"+ (index==TabIdx? " active":"")} href="javascript:void(0);" onClick={index==TabIdx ? null: this.onTab.bind(this, index)}>{item}</a>
            </li>;    // 取得陣列中雙數的物件
            }.bind(this));                        

            return (
                <div>
                    <h4>Customer Information</h4>
                    <div className="shadow p-3 mb-5 bg-white rounded">
                        <ul className="nav nav-tabs ">
                            {tabBtn}
                        </ul>
                        {dataContainer}
                    </div>
                </div>                
            )
        }
    }

    ReactDOM.render(<CustInfoPanel/>,document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios);