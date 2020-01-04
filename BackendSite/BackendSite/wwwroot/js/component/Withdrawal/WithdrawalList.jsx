import {number_format} from '../commJS/common';
import {Pagination} from '../commJS/Pagination.jsx';

(function ($, React, ReactDOM, i18n, axios, moment, Calendar) {
    class WithdrawalList extends React.Component {
        constructor(props) {
            super(props);

        }

        render() {
            let rows = this.props.searchData.data.map(row => 
                <tr key={row.transId}>
                    <th scope="row">{row.transId}</th>
                    <td>{row.withdrawalType}</td>
                    <td>{row.userName}</td>
                    <td>{row.withdrawalInfo}</td>
                    <td>{number_format(row.amount)}</td>
                    <td>{row.chargeFee}</td>
                    <td>{row.memo}</td>
                    <td>{moment(row.createTime).format('l LTS')}</td>
                    <td>{row.status}</td>
                </tr>
            );

            return (
                <div>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">{i18n.lbl_WithdrawalId}</th>
                                <th scope="col">{i18n.lbl_WithdrawalType}</th>
                                <th scope="col">{i18n.lbl_UserName}</th>
                                <th scope="col">WithdrawalInfo</th>
                                <th scope="col">{i18n.lbl_Amount}</th>
                                <th scope="col">{i18n.lbl_ChargeFee}</th>
                                <th scope="col">{i18n.lbl_Memo}</th>
                                <th scope="col">{i18n.lbl_CreateTime + " (" + moment().format('Z') + ")"}</th>
                                <th scope="col">{i18n.lbl_Status}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <Pagination Data={{...this.props.searchData}} ChangePage={this.props.ChangePage}/>
                </div>
            )
        }
    }

    class WithdrawalSearchForm extends React.Component {
        constructor(props) {
            super(props);
            this.f_trigger_Start = React.createRef();
            this.selectdate_start = React.createRef();
            this.f_trigger_End = React.createRef();
            this.selectdate_end = React.createRef();

            this.state={selectType:"", selectCurrency:"", selectStatus:"1", SelectUser:"", SelectWithdrawalID:""};
        }

        onRefresh() {
            let searchData = this.state.searchData;
            this.onSearch(searchData.pageNumber);
        }

        onChange(event) {
            let obj={};
            obj[event.target.name]=event.target.value;
            this.setState(obj);
        }

        onSearch(pageNumber) {
            //Loading.OnLoading();
            let endDate = moment(this.selectdate_end.current.value).add(1, 'days').unix()-1;
            let data = {StartDate: moment(this.selectdate_start.current.value).unix(), EndDate: endDate, PageNumber: pageNumber};
            if(this.state.SelectWithdrawalID!=""){
                data.WithdrawalId = this.state.SelectWithdrawalID;
            }

            if(this.state.selectType!=""){
                data.withdrawalType = parseInt(this.state.selectType);
            }

            if(this.state.selectStatus!=""){
                data.status = parseInt(this.state.selectStatus);
            }

            if(this.state.SelectUser!=""){
                data.UserName = this.state.SelectUser;
            }

            if(this.state.selectCurrency!=""){
                data.CurrencyID = parseInt(this.state.selectCurrency);
            }

            axios.post('/api/Withdrawal/GetWithdrawalRequestList', data)
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
            this.onSearch(1);
        }

        componentDidMount() {
            let f_trigger_Start = this.f_trigger_Start.current;
            let f_trigger_End = this.f_trigger_End.current;
            let selectdate_end = this.selectdate_end.current;
            let selectdate_start = this.selectdate_start.current;

            Calendar.setup({
                inputField: selectdate_end, // id of the input field
                ifFormat: "mm/dd/y",        // format of the input field
                button: f_trigger_End,  // trigger for the calendar (button ID)
                singleClick: true              // double-click mode
            });

            Calendar.setup({
                inputField: selectdate_start, // id of the input field
                ifFormat: "mm/dd/y",        // format of the input field
                button: f_trigger_Start,  // trigger for the calendar (button ID)
                singleClick: true              // double-click mode
            });            
        }

        render() {
            let selectCurrency = CurrencyList.map(function(obj) { 
                return <option key={obj} value={obj}>{i18n["lbl_Currency"+obj]}</option>;
            });

            selectCurrency.splice(0 ,0 , <option key={""} value={""}>{"All"}</option>)
            selectCurrency = <select name="selectCurrency" onChange={this.onChange.bind(this)} value={this.state.selectCurrency}>{selectCurrency}</select>;
            let searchData = this.state.searchData;

            return (
                <div>
                    <form method="post" className="shadow p-3 mb-5 bg-white rounded" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="boxgy">
                            <table border="0" cellPadding="5" cellSpacing="0">
                                <tbody>
                                    <tr>
                                        <td className="title11bk">
                                            <img src="/css/Oneworks/images/icon_arrowdb_rd.gif" width="13" height="6" />Select Type of withdrawal
                                            <select name="selectType" onChange={this.onChange.bind(this)} value={this.state.selectType}>
                                                <option value="">All</option>
                                                <option value="0">Undefine</option>
                                                <option value="1">Local Bank</option>
                                                <option value="2">Online Payment</option>
                                            </select>
                                        </td>
                                        <td className="title11bk">
                                            <img src="/css/Oneworks/images/icon_arrowdb_rd.gif" width="13" height="6" />Select date of withdrawal
                                            <input ref={this.selectdate_start} type="text" defaultValue={moment().add(-10, 'days').format('l')} size="8"/>
                                            <a><img ref={this.f_trigger_Start} src="/css/Oneworks/images/icon_date.gif" width="21" height="14" border="0"/></a>to
                                            <input ref={this.selectdate_end} type="text" defaultValue={moment().format('l')} size="8" />
                                            <a><img ref={this.f_trigger_End} src="/css/Oneworks/images/icon_date.gif" width="21" height="14" border="0"/></a>
                                        </td>
                                        {/*<td style={{width:"10px"}}>
                                            <div id="ExportIcon">
                                                <a><img id="ImgExcel" name="ImgExcel" src="/css/Oneworks/images/excel.gif" border="0"/></a>
                                            </div>
                                        </td>*/}
                                    </tr>
                                    <tr>
                                        <td className="title11bk">
                                            <img src="/css/Oneworks/images/icon_arrowdb_rd.gif" width="13" height="6" />Select currency of withdrawal
                                                {selectCurrency}
                                        </td>
                                        <td className="title11bk">
                                            <img src="/css/Oneworks/images/icon_arrowdb_rd.gif" width="13" height="6" />Select type of withdrawal status
                                            <select name="selectStatus" onChange={this.onChange.bind(this)} value={this.state.selectStatus}>
                                                <option value="">All</option>
                                                <option value="1">Pending</option>
                                                <option value="2">On Hold</option>
                                                <option value="3">Verify</option>
                                                <option value="4">Process</option>
                                                <option value="5">Cancelled</option>
                                                <option value="6">Approved</option>
                                                <option value="7">Rejected</option>
                                            </select>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td className="title11bk">
                                            <img src="/css/Oneworks/images/icon_arrowdb_rd.gif" width="13" height="6" />Select User
                                            <input type="text" name="SelectUser" value={this.state.SelectUser} onChange={this.onChange.bind(this)}/>
                                        </td>
                                        <td className="title11bk">
                                            <img src="/css/Oneworks/images/icon_arrowdb_rd.gif" width="13" height="6" />Select Withdrawal ID
                                            <input type="text" name="SelectWithdrawalID" value={this.state.SelectWithdrawalID} onChange={this.onChange.bind(this)}/>
                                        </td>
                                    </tr>
                                    {/*<tr>
                                        <td className="title11bk">
                                            <img src="/css/Oneworks/images/icon_arrowdb_rd.gif" width="13" height="6" />Select Bank Code
                                            <select name="selectBankType">
                                                <option value="">All</option>
                                                <option value="1">abc</option>
                                                <option value="2">qwe</option>
                                                <option value="4">win365</option>
                                                <option value="14">xxx</option>
                                            </select>
                                        </td>
                                    </tr>*/}
                                </tbody>
                            </table>
                            <div className="btnarea">
                                <div className="btnstyle" style={{textAlign:"center"}}>
                                    <ul>
                                        <li><button className="btnbu" type="submit"><span>Search</span></button></li>
                                        <li><button className="btngy" type="Reset"><span>Reset</span></button></li>
                                        {/*<li><a href="javascript:ReportToExport();" style="" className="btnbu"><span>Export</span></a></li>*/}
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </form>
                    {searchData !=null ? <WithdrawalList searchData={searchData} ChangePage={this.onSearch.bind(this)}/> : null}
                </div>
            )
        }
    }
    
    ReactDOM.render(<WithdrawalSearchForm/>,document.getElementById('searchForm'));
    })(jQuery, React, ReactDOM, i18n, axios, moment, Calendar);
    