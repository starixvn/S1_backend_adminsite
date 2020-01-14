import {number_format, getUrlParameter} from '../commJS/common';

(function ($, React, ReactDOM, i18n, axios) {
    class CustInfo extends React.Component {
        constructor(props) {
            super(props);
            this.state = {};
        }

        render() {

            return (
                <div>
                    <table class="table table-detail">
                        <tbody>
                            <tr>
                                <th scope="row">User ID</th>
                                <td>Nova88</td>
                            </tr>
                            <tr>
                                <th scope="row">First name</th>
                                <td>Luke</td></tr>
                            <tr>
                                <th scope="row">Last Name</th>
                                <td>Davids</td></tr>
                            <tr>
                                <th scope="row">Gender</th>
                                <td>Male</td></tr>
                            <tr>
                                <th scope="row">Date Of Birth</th>
                                <td>12/01/1988</td>
                            </tr>
                            <tr>
                                <th scope="row">Mobile</th>
                                <td>090 1234 567</td>
                            </tr>
                            <tr>
                                <th scope="row">Country</th>
                                <td>Thailand</td></tr>
                            <tr>
                                <th scope="row">Address</th>
                                <td>Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.</td>
                            </tr>
                            <tr>
                                <th scope="row">City/Town</th>
                                <td>Bangkok</td>
                            </tr>
                            <tr>
                                <th scope="row">State</th>
                                <td></td>
                            </tr>
                            <tr>
                                <th scope="row">Zip Code</th>
                                <td></td>
                            </tr>
                            <tr>
                                <th scope="row">My preferred username</th>
                                <td></td>
                            </tr>
                            <tr>
                                <th scope="row">My preferred currency</th>
                                <td></td>
                            </tr>
                            <tr>
                                <th scope="row">My preferred odds</th>
                                <td></td>
                            </tr>
                            <tr>
                                <th scope="row">My preferred languages</th>
                                <td>English</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            )
        }
    }
    
    class CustInfoPanel extends React.Component {
        _Tabs=["Customer Info","Financial Log","Deposit Info","Transfer Info","Withdrawal Info","Login Log", "Note"];

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
            </li>;
            }.bind(this));                        

            return (
                <div>
                    <h4 className="page-title">Customer Information</h4>
                    <div className="p-20">
                        <div className="row">
                            <div className="info-tab">
                                <div className="nav flex-column nav-pills" id="v-pills-tab" role="tablist" aria-orientation="vertical">
                                    <div className="user">
                                        <img className="avatar" src="/images/avatar.png" alt="User image" />
                                        <span className="name">Luke Davids</span>
                                    </div>
                                    {tabBtn}
                                </div>
                            </div>
                            <div className="info-content">
                                <div className="tab-content" id="v-pills-tabContent">
                                    {dataContainer}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>                
            )
        }
    }

    ReactDOM.render(<CustInfoPanel/>,document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios);