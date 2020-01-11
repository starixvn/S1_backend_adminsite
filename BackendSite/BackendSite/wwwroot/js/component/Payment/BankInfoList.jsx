import {number_format} from '../commJS/common';

(function ($, React, ReactDOM, i18n, axios, moment) {
    class BankCardInfoList extends React.Component {
        constructor(props) {
            super(props);

        }

        onRefresh() {
            this.props.onRefresh();
        }

        render() {
            let rows = this.props.searchData.data.map(row => 
                <tr key={row.bankId}>
                    <th scope="row">{row.siteName}</th>
                    <td scope="row">{i18n["lbl_Currency"+row.currencyId]}</td>
                    <td>{row.bankCode}</td>
                    <td>{row.bankName}</td>
                    <td>{row.accountNo}</td>
                    <td>{row.accountName}</td>                    
                    <td>{row.pointLevel}</td>                    
                    <td className="text-right">{number_format(row.minAmount)}</td>
                    <td className="text-right">{number_format(row.maxAmount)}</td>
                    <td className="text-right">{number_format(row.dailyLimitAmount)}</td>
                    <td>{row.supportMethod}</td>
                </tr>
            );

            return (
                <div>
                    <button type="button" className="btn btn-secondary float-right" onClick={this.onRefresh.bind(this)}>Refresh</button>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">Site Name</th>
                                <th scope="col">CCY</th>
                                <th scope="col">Bank Code</th>
                                <th scope="col">Bank Name</th>
                                <th scope="col">Acc. NO.</th>
                                <th scope="col">Acc. Name</th>
                                <th scope="col">VIP Level</th>                                
                                <th scope="col" className="text-right">Min Amount</th>
                                <th scope="col" className="text-right">Max Amount</th>
                                <th scope="col" className="text-right">Daily Limit Amount</th>
                                <th scope="col">Support Method</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                </div>
            )
        }
    }

    class BankCardForm extends React.Component {
        constructor(props) {
            super(props);

            this.state={};
        }

        closeForm() {
            this.props.closeForm();
        }

        render() {
            let searchData = this.state.searchData;

            return (
                <form method="post" className="form-horizontal">
                    <div className="form-group row">                
                        <label className="col-sm-2 col-form-label">Site Name</label>
                        <div className="form-check form-check-inline">
                            <input type="radio" className="form-check-input" id="A" name="SiteName"/>
                            <label className="form-check-label" for="A">1</label>
                        </div>
                        <div className="form-check form-check-inline">
                            <input type="radio" className="form-check-input" id="B" name="SiteName"/>
                            <label className="form-check-label" for="B">1</label>
                        </div>                        
                    </div>
                    <div className="form-group row">
                        <label className="col-sm-2 col-form-label">CCY</label>
                        <div className="col-sm-10">
                            <select id="CurrencyId" className="custom-select mr-sm-2">
                                <option value="@item.value">@item.text</option>
                         </select>
                        </div>
                    </div>
                    <div className="form-group row">
                        <label className="col-sm-2 col-form-label">@selLangBLL.GetResourceValue("lbl_BankName")</label>
                        <div className="col-sm-10">
                            <select id="BankCode" className="custom-select mr-sm-2">
                                <option id="SysId" name="SysId" value="@item.SysId">@item.BankName</option>
                            </select>
                        </div>
                    </div>
                    <div className="form-group row">
                        <label className="col-sm-2 col-form-label">@selLangBLL.GetResourceValue("lbl_AccountName")</label>
                        <div className="col-sm-10">
                            <input type="text" className="form-control" id="AccountName" placeholder=""/>
                        </div>
                    </div>
                    <div className="form-group row">
                        <label className="col-sm-2 col-form-label">@selLangBLL.GetResourceValue("lbl_AccountNo")</label>
                        <div className="col-sm-10">
                            <input type="text" className="form-control" id="AccountNo" placeholder=""/>
                        </div>
                    </div>
                    <div className="form-group row">
                        <label className="col-sm-2 col-form-label">@selLangBLL.GetResourceValue("lbl_MinAmount")</label>
                        <div className="col-sm-10">
                            <input type="text" className="form-control" id="MinAmount" placeholder=""/>
                        </div>
                    </div>
                    <div className="form-group row">
                        <label className="col-sm-2 col-form-label">@selLangBLL.GetResourceValue("lbl_MaxAmount")</label>
                        <div className="col-sm-10">
                            <input type="text" className="form-control" id="MaxAmount" placeholder=""/>
                        </div>
                    </div>
                    <div className="form-group row">
                        <label className="col-sm-2 col-form-label">@selLangBLL.GetResourceValue("lbl_PointLevel")</label>
                        <div className="col-sm-10">
                            <input type="text" className="form-control" id="PointLevel" placeholder=""/>
                        </div>
                    </div>
                    <div className="form-group row">
                        <div className="col-sm-10">
                            <button type="submit" className="btn btn-primary" onClick={this.closeForm.bind(this)}>Back</button>
                            <button type="submit" className="btn btn-primary">Create</button>
                        </div>
                    </div>
                </form>
            )
        }
    }        

    class BankCardInfoPanel extends React.Component {
        constructor(props) {
            super(props);

            this.state={SiteId: 1, ShowForm: false};
        }

        onRefresh() {
            this.onSearch();
        }

        onChange(event) {
            let obj={};
            obj[event.target.name]=event.target.value;
            this.setState(obj);
        }

        onSearch() {
            //Loading.OnLoading();
            let data = {SiteId: this.state.SiteId};

            /*if(this.state.selectCurrency!=""){
                data.CurrencyID = parseInt(this.state.selectCurrency);
            }*/

            axios.post('/api/BankList/GetLoaclBankList', data)
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({searchData: result, GetData: true});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        handleSubmit(event) {
            event.preventDefault();
            //this._Search = true;
            this.setState(data, function(){
                this.onSearch(1);
            }.bind(this));
        }

        showForm(){
            this.setState({ShowForm: true});
        }

        closeForm(){
            this.setState({ShowForm: false});
        }

        componentDidMount() {
            this.onSearch();
        }

        onCheckDetail(id) {
            this.setState({transId: id, ShowDetial: true});
        }

        onBack() {
            this.setState({ShowDetial: false});
            this.onRefresh();
        }

        render() {
            let searchData = this.state.searchData;
            let dataContainter = [];
            if(searchData!=null) {
                
            }

            return (
                this.state.ShowForm ? <BankCardForm closeForm={this.closeForm.bind(this)}/> : 
                <div>
                    <button onClick={this.showForm.bind(this)}>Add Bank Card</button>
                    {searchData&&searchData.data!=null?<BankCardInfoList ChangePage={this.onSearch.bind(this)} searchData={searchData} onRefresh={this.onRefresh.bind(this)}/>:null}
                </div>
            )
        }
    }
    
    ReactDOM.render(<BankCardInfoPanel/>, document.getElementById('BankCardInfoPanel'));
    })(jQuery, React, ReactDOM, i18n, axios, moment);