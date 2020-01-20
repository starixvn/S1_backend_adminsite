import {number_format} from '../commJS/common';
import {Pagination} from '../commJS/Pagination.jsx';

(function ($, React, ReactDOM, i18n, axios, moment, Calendar) {
    const WithdrawalStatus = {
        Pending:1,
        OnHold :2,
        Verify:3,
        Process:4,
        Cancelled:5,
        Approved:6,
        Rejected :7
    };

    const WithdrawalStatusOp = {
        1:<span className="status status-pending">{i18n["lbl_WithdrawalStatus1"]}</span>,
        2:<span className="status status-hold">{i18n["lbl_WithdrawalStatus2"]}</span>,
        3:<span className="status status-verify">{i18n["lbl_WithdrawalStatus3"]}</span>,
        4:<span className="status status-process">{i18n["lbl_WithdrawalStatus4"]}</span>,
        5:<span className="status status-cancelled">{i18n["lbl_WithdrawalStatus5"]}</span>,
        6:<span className="status status-approved">{i18n["lbl_WithdrawalStatus6"]}</span>,
        7:<span className="status status-rejected">{i18n["lbl_WithdrawalStatus7"]}</span>,
    }

    class WithdrawalList extends React.Component {
        constructor(props) {
            super(props);

        }

        onCheckDetail(id) {
            this.props.onCheckDetail(id);
        }

        onRefresh() {
            this.props.onRefresh();
        }

        render() {
            let rows = this.props.searchData.data.map(row => 
                <tr key={row.transId}>
                    <th scope="row"><a href="#" role="button" onClick={this.onCheckDetail.bind(this, row.transId)}>{row.transId}</a></th>
                    <td>{i18n["lbl_WithdrawalType" + row.withdrawalType]}</td>
                    <td><a href={"/CustManage/CustInfo?UserName="+row.userName+"&custId="+ row.custId +"&TransId=" + row.transId} target="_blank">{row.userName}</a></td>
                    <td>{i18n["lbl_Currency"+row.currencyId]}</td>
                    <td className="text-right">{number_format(row.amount)}</td>                    
                    <td>{row.accountNo}</td>
                    <td>{row.accountName}</td>
                    <td>{row.memo}</td>
                    <td>{moment(row.createTime).format('l LTS')}</td>
                    <td>{WithdrawalStatusOp[row.status]}</td>
                </tr>
            );

            let RefreshBtn = null;
            if(this.props.onRefresh == null) {
                RefreshBtn = <button type="button" className="btn btn-refresh" disabled={true} onClick={null}>
                            Loading
                        </button>;
            } else {
                RefreshBtn = <button type="button" className="btn btn-refresh" onClick={this.onRefresh.bind(this)}>
                            Refresh <span>({this.props.timeInterval})</span>
                        </button>;
            }
            return (
                <div className="table">
                    <div className="table-options">
                        {RefreshBtn}
                    </div>
                    
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">Trxn.ID</th>
                                <th scope="col">{i18n.lbl_WithdrawalType}</th>
                                <th scope="col">{i18n.lbl_UserName}</th>
                                <th scope="col">CCY</th>
                                <th scope="col" className="text-right">{i18n.lbl_Amount}</th>
                                <th scope="col">To Acc. NO.</th>
                                <th scope="col">To Acc. Name</th>                                
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

    class WithdrawalRequestComment  extends React.Component {
        constructor(props) {
            super(props);
        }
        render() {
            let data = this.props.Data;
            let i = 1;
            let rows = null;
            if(data !=null) {
                rows = data.map(function(obj) { 
                    return <tr key={i}>
                                    <td scope="row">{i++}</td>
                                    <td>{moment(obj.createTime).format('l LTS')}</td>
                                    <td>{obj.comment}</td>
                                    <td>{i18n["lbl_WithdrawalStatus"+obj.action]}</td>
                                    <td>{obj.operator}</td>
                                </tr>;
                });
            }

            return (
                data !=null ? 
        <table className="table table-striped">
            <thead>
                <tr>
                    <th scope="col">{i18n.lbl_No}</th>
                    <th scope="col">{i18n.lbl_CreateTime}</th>
                    <th scope="col">{i18n.lbl_Comment}</th>
                    <th scope="col">{i18n.lbl_Action}</th>
                    <th scope="col">Operator</th>
                </tr>
            </thead>
            <tbody>
                {rows}
            </tbody>
        </table> : null
            )
        }
    }            

    class WithdrawalDetail extends React.Component {
        _clipboard=null;
        constructor(props) {
            super(props);
            this.state={Data:null};
        }

        componentDidMount() {
            this.onGetData();
            this.onGetComment();
            this._clipboard = new ClipboardJS(".js-copy");

            this._clipboard.on('success', function(e) {
                //console.log(e);
            });
            this._clipboard.on('error', function(e) {
                //console.log(e);
            });            
        }

        onGetData() {
            //Loading.OnLoading();
            axios.post('/api/Withdrawal/GetWithdrawalRequest', { transId: this.props.transId})
            .then(async function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {

                    if(result.data.status==3) {
                        await this.onGetPayList(result.data.currencyId, result.data.sysId);
                        await this.onGetBankInfoOffList(result.data.currencyId, result.data.sysId);
                    }
                    
                    let SelId = null;
                    if(result.data.withdrawalType==1) {
                        SelId = result.data.payId;
                    } else if (result.data.withdrawalType==2) {
                        SelId = result.data.bankId;
                    }

                    this.setState({Data: result.data, selectType: result.data.withdrawalType, GetData: true,SelId: SelId,Comment: result.data.comment});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        onBack() {
            this.props.onBack();
        }

        setComment(event) {
            this.setState({Comment: event.target.value});
        }

        onGetComment(){
            axios.post('/api/Withdrawal/GetWithdrawalRequestComment', { transId: this.props.transId})
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({Comments: result.data, GetData: true});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));    
        }

        onConfirm(action){
            if((action != 0 && confirm('Are you sure?')) || action==0) {
                let data = { transId: this.props.transId, Comment : this.state.Comment, Action:action};
                
                if(action == 4) {
                    data.WithdrawalInfo = this.state.Data.withdrawalInfo;
                    data.SelId = parseInt(this.state.SelId);
                    data.WithdrawalType = this.state.selectType;
                }

                axios.post('/api/Withdrawal/WithdrawalConfirm', data)
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        alert(result.message);
                        if(action!=0) {
                            this.onBack();
                        } else {
                            this.onGetComment();
                        }
                    } else {
                        alert(result.message);
                        this.onGetComment();
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
            } 
        }

        onSelectType(event) {
            let SelId=null;

            if(parseInt(event.target.value)==1 && this.state.PayList.length>0) {
                SelId=this.state.PayList[0].payId;
            } else if(parseInt(event.target.value)==2 && this.state.BankInfoOffList.length>0) {
                SelId=this.state.BankInfoOffList[0].bankId;
            }

            this.setState({selectType: parseInt(event.target.value), SelId: SelId});
        }

        async onGetBankInfoOffList(currencyId, sysId) {
            let data = this.state.Data;
            await axios.post('/api/Withdrawal/GetBankInfoOffList', { CurrencyId: currencyId, SysId: sysId})
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({BankInfoOffList: result.data});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }

        async onGetPayList(currencyId, sysId) {
            let data = this.state.Data;
            await axios.post('/api/Withdrawal/GetPayList', { CurrencyId: currencyId, SysId: sysId})
            .then(function (response) {
                //Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({PayList: result.data});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                //Loading.UnLoading();
                alert(error);
            }.bind(this));
        }        

        onSelId(event) {
            this.setState({SelId: event.target.value });
        }

        render() {
            let data = this.state.Data;
            let btnGroup = [];
            let selectType = null
            let selectPay = null;
            let PayFrom = [];
            if (data != null) {
                if (data.status == 1 || data.status == 2 || data.status == 3 || data.status == 4)
                    btnGroup.push(<button key={0} type="button" className="btn btn-draft" onClick={this.onConfirm.bind(this, 0)}>Draft</button>);
                if (data.status == 1 || data.status == 3)
                    btnGroup.push(<button key={2} type="button" className="btn btn-hold" onClick={this.onConfirm.bind(this, 2)}>On Hold(Risk Check)</button>);
                if (data.status == 1 || data.status == 2 || data.status == 4)
                    btnGroup.push(<button key={3} type="button" className="btn btn-verify" onClick={this.onConfirm.bind(this, 3)}>Verify</button>);
                if (data.status == 3)
                    btnGroup.push(<button key={4} type="button" className="btn btn-process" onClick={this.onConfirm.bind(this, 4)}>Process</button>);
                if (data.status == 4 && data.withdrawalType != 1)
                    btnGroup.push(<button key={6} type="button" className="btn btn-aprroved" onClick={this.onConfirm.bind(this, 6)}>Approved</button>);
                if ((data.status == 3 || data.status == 2))
                    btnGroup.push(<button key={7} type="button" className="btn btn-rejected" onClick={this.onConfirm.bind(this, 7)}>Rejected</button>);

                selectType = i18n["lbl_WithdrawalType" +data.withdrawalType];

                if(data.status==3) {
                    let selectOp = [];
                    selectOp.push(<option key={0} value="0">Undefine</option>);
                     if(this.state.PayList.length > 0)
                        selectOp.push(<option key={1} value="1">Online Payment</option>);
                     if(this.state.BankInfoOffList.length > 0)
                        selectOp.push(<option key={2} value="2">Local Bank</option>);
                    selectType = <select className="form-control" onChange={this.onSelectType.bind(this)} value={this.state.selectType}>
                        {selectOp}
                    </select>;

                    selectPay = null;
                    if(this.state.selectType==1) {
                        selectPay = this.state.PayList.map(row => <option key={row.payId} value={row.payId}>{row.payName}</option>);
                        /*if(this.state.PayList.length>0) {
                            this.state.SelId=this.state.PayList[0];
                        }*/                       
                    } else if(this.state.selectType==2) {
                        selectPay = this.state.BankInfoOffList.map(row => <option key={row.bankId} value={row.bankId}>{row.bankCode + "-"  + row.accountName + "-" + row.accountNo}</option>);
                        /*if(this.state.BankInfoOffList.length>0) {
                            this.state.SelId=this.state.BankInfoOffList[0];
                        }*/
                    }

                    if(selectPay!=null){
                        selectPay = <tr>
                            <th scope="row">select Pay</th>
                            <td><select className="form-control" value={this.state.SelId} onChange={this.onSelId.bind(this)}>{selectPay}</select></td>
                        </tr>;
                    }
                } else {
                    if(data.payId > 0) {
                        PayFrom.push(<tr key={1}>
                            <th scope="row">Payment</th>
                            <td>{data.payName}</td>
                        </tr>);
                    } else if(data.bankId > 0) {
                        PayFrom.push(<tr key={1}>
                            <th scope="row">From Bank Code</th>
                            <td>{data.fromBankCode}</td>
                        </tr>);

                        PayFrom.push(<tr key={2}>
                            <th scope="row">From Bank Name</th>
                            <td>{data.fromBankName}</td>
                        </tr>);
                        let bankInfoObj = JSON.parse(data.withdrawalInfo);
                       PayFrom.push(<tr key={3}>
                            <th scope="row">From Account Name</th>
                            <td>{bankInfoObj.FromAccountName}</td>
                        </tr>);
                       PayFrom.push(<tr key={4}>
                            <th scope="row">From Account No</th>
                            <td>{bankInfoObj.FromAccountNo}</td>
                        </tr>);                                     
                    }
                }
            }

            return (
                data != null ? <div>
                    <table className="table table-detail">
                        <tbody>
                            <tr>
                                <th scope="row">SiteName</th>
                                <td>{data.siteName}</td>
                            </tr>
                            <tr>
                                <th scope="row">Transaction No.</th>
                                <td>{data.transId}</td>
                            </tr>
                            <tr>
                                <th scope="row">Status</th>
                                <td>{i18n["lbl_WithdrawalStatus" + data.status]}</td>
                            </tr>
                            <tr>
                                <th scope="row">Username</th>
                                <td><a href={"/CustManage/CustInfo?UserName="+data.userName+"&custId="+ data.custId +"&TransId=" + data.transId} target="_blank">{data.userName}</a></td>
                            </tr>
                            <tr>
                                <th scope="row">E-mail</th>
                                <td>{data.email}</td>
                            </tr>
                            <tr>
                                <th scope="row">{"Date" + " (" + moment().format('Z') + ")"}</th>
                                <td>{moment(data.createTime).format('l LTS')}</td>
                            </tr>
                            <tr>
                                <th scope="row">Name of Bank</th>
                                <td>{data.bankName}</td>
                            </tr>
                            <tr>
                                <th scope="row">Account No</th>
                                <td>
                                    {data.accountNo}
                                    <button type="button" className="btn btn-copy js-copy" data-clipboard-text={data.accountNo}>Copy</button>
                                </td>
                            </tr>
                            <tr>
                                <th scope="row">Account Name</th>
                                <td>{data.accountName}<button type="button" className="btn btn-copy js-copy" data-clipboard-text={data.accountName}>Copy</button></td>
                            </tr>
                            <tr>
                                <th scope="row">Currency</th>
                                <td>{i18n["lbl_Currency" + data.currencyId]}</td>
                            </tr>
                            <tr>
                                <th scope="row">Request Amount</th>
                                <td>{number_format(data.amount)}<button type="button" className="btn btn-copy js-copy" data-clipboard-text={data.amount}>Copy</button></td>
                            </tr>
                            <tr>
                                <th scope="row">Type</th>
                                <td>{selectType}</td>
                            </tr>
                            {selectPay}
                            {PayFrom}
                            <tr>
                                <th scope="row">Ref ID</th>
                                <td>{data.refId}</td>
                            </tr>
                        </tbody>
                    </table>
                    <WithdrawalRequestComment Data={this.state.Comments} />
                    <div className="input-field mt-20">
                        <label htmlFor="command" className="label">{i18n.lbl_Comment}</label>
                        <textarea id="command" className="form-control" rows="2" value={this.state.Comment} onChange={this.setComment.bind(this)} />
                    </div>
                    <div className="button-group mt-10">
                        <button type="button" className="btn btn-back pull-left" onClick={this.onBack.bind(this)}>Back</button>
                        <div className="pull-right">{btnGroup}</div>
                        <div className="clearfix"></div>
                    </div>
                    
                </div> : null
            )
        }
    }    

    class WithdrawalPanel extends React.Component {
        _myTimer;
        constructor(props) {
            super(props);
            this.f_trigger_Start = React.createRef();
            this.selectdate_start = React.createRef();
            this.f_trigger_End = React.createRef();
            this.selectdate_end = React.createRef();

            this.state={selectType:"", selectCurrency:"", selectStatus:"1", SelectUser:"", SelectWithdrawalID:"", ShowDetial: false, timeInterval:60 ,IsRefresh: false };
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
            this.setState({IsRefresh:true});
            if(this._myTimer!= null){
                clearTimeout(this._myTimer);
            }
            //Loading.OnLoading();
            let data = {StartDate: this.state.StartDate, EndDate: this.state.EndDate, PageNumber: pageNumber};
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
                this.AutoRefresh();                
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({searchData: result, GetData: true, IsRefresh:false, timeInterval: 60});
                } else {
                    alert(result.message);
                    this.setState({IsRefresh:false,timeInterval: 60});
                }
            }.bind(this)).catch(function (error) {
                this.AutoRefresh();
                //Loading.UnLoading();
                this.setState({IsRefresh:false,timeInterval: 60});
                alert(error);
            }.bind(this));
        }

        handleSubmit(event) {
            event.preventDefault();
            //this._Search = true;
            let endDate = moment(this.selectdate_end.current.value).add(1, 'days').unix()-1;
            let data = {StartDate: moment(this.selectdate_start.current.value).unix(), EndDate: endDate};

            this.setState(data, function(){
                this.onSearch(1);
            }.bind(this));
        }

        AutoRefresh() {
            if(this.state.timeInterval==0){
                this.onRefresh();
            } else {
                let timeInterval = this.state.timeInterval;
                timeInterval--;
                this.setState({timeInterval:timeInterval});
                this._myTimer = setTimeout(function(){
                    this.AutoRefresh();
                }.bind(this), 1000);
            }
        }

        componentDidMount() {
            let f_trigger_Start = this.f_trigger_Start.current;
            let f_trigger_End = this.f_trigger_End.current;
            let selectdate_end = this.selectdate_end.current;
            let selectdate_start = this.selectdate_start.current;

                //selectdate_start.value
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

            //setTimeout(thisAutoRefresh, 2000);
        }

        onCheckDetail(id) {
            this.setState({transId: id, ShowDetial: true});
        }

        onBack() {
            this.setState({ShowDetial: false});
            this.onRefresh();
        }

        render() {
            let selectCurrency = CurrencyList.map(function(obj) { 
                return <option key={obj} value={obj}>{i18n["lbl_Currency"+obj]}</option>;
            });

            selectCurrency.splice(0 ,0 , <option key={""} value={""}>{"All"}</option>)
            selectCurrency = <select name="selectCurrency" onChange={this.onChange.bind(this)} value={this.state.selectCurrency}>{selectCurrency}</select>;
            let searchData = this.state.searchData;
            let StartDate = this.state.StartDate;
            let EndDate = this.state.EndDate;
            return (
                <div>
                    {!this.state.ShowDetial ? <form method="post" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="filter">
                            <div className="row-flex">
                                <div className="input-item">
                                    <span className="label">Type</span>
                                    <div className="field">
                                        <select name="selectType" onChange={this.onChange.bind(this)} value={this.state.selectType}>
                                            <option value="">All</option>
                                            <option value="0">Undefine</option>
                                            <option value="1">Online Payment</option>
                                            <option value="2">Local Bank</option>
                                        </select>
                                    </div>
                                </div>

                                <div className="input-item">
                                    <span className="label">From Date</span>
                                    <div className="date">
                                        <input ref={this.selectdate_start} type="text" defaultValue={StartDate != null ? moment.unix(StartDate).format('l') : moment().add(-10, 'days').format('l')} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_Start}>{/*<img ref={this.f_trigger_Start} src="/css/Oneworks/images/icon-calendar.png" />*/}</a>
                                    </div>
                                </div>

                                <div className="input-item">
                                    <span className="label">To Date</span>
                                    <div className="field date">
                                        <input ref={this.selectdate_end} type="text" defaultValue={EndDate != null ? moment.unix(EndDate).format('l') : moment().format('l')} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_End}>{/*<img ref={this.f_trigger_End} src="/css/Oneworks/images/icon-calendar.png" />*/}</a>
                                    </div>
                                </div>

                                <div className="input-item">
                                    <span className="label">Currency</span>
                                    <div className="field currency">
                                        {selectCurrency}
                                    </div>
                                </div>

                                <div className="input-item">
                                    <span className="label">Status</span>
                                    <div className="field">
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
                                    </div>
                                </div>
                            </div>

                            <div className="row-flex">
                                <div className="input-item">
                                    <span className="label">User</span>
                                    <div className="field">
                                        <input type="text" name="SelectUser" value={this.state.SelectUser} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>

                                <div className="input-item">
                                    <span className="label">Withdrawal ID</span>
                                    <div className="field">
                                        <input type="text" name="SelectWithdrawalID" value={this.state.SelectWithdrawalID} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>

                                <div className="button-group">
                                    <button className="btn btn-search" type="submit"><span>Search</span></button>
                                </div>
                            </div>
                        </div>
                    </form> : null}
                    {searchData !=null && !this.state.ShowDetial ? <WithdrawalList timeInterval={this.state.timeInterval} searchData={searchData} ChangePage={this.onSearch.bind(this)} onCheckDetail={this.onCheckDetail.bind(this)} onRefresh={!this.state.IsRefresh ? this.onRefresh.bind(this) : null} /> : null}
                    {this.state.transId != null && this.state.ShowDetial ? <WithdrawalDetail transId={this.state.transId} onBack={this.onBack.bind(this)}/> : null}
                </div>
            )
        }
    }
    
    ReactDOM.render(<WithdrawalPanel/>,document.getElementById('searchForm'));
    })(jQuery, React, ReactDOM, i18n, axios, moment, Calendar);
    