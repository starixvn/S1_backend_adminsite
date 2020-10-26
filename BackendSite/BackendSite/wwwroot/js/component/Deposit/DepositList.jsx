import { number_format } from '../commJS/common';
import { Pagination } from '../commJS/Pagination.jsx';
import * as Loading from '../commJS/Loading.jsx';
import DateTimePicker from 'react-datetime';

(function ($, React, ReactDOM, i18n, axios, moment, Calendar) {
    const DepositStatus = {
        Pending: 1,
        Paid: 2,
        Approved: 3,
        Rejected: 4,
        Canceled: 5,
        Expired: 6,
        OnHold: 7
    };

    const DepositStatusOp = {
        1: <span className="status status-pending">{i18n["lbl_DepositStatus1"]}</span>,
        2: <span className="status status-">Paid</span>,
        3: <span className="status status-approved">{i18n["lbl_DepositStatus3"]}</span>,
        4: <span className="status status-rejected">{i18n["lbl_DepositStatus4"]}</span>,
        5: <span className="status status-cancelled">{i18n["lbl_DepositStatus5"]}</span>,
        6: <span className="status status-">Expired</span>,
        7: <span className="status status-hold">{i18n["lbl_DepositStatus7"]}</span>,
    }

    const TypeSelectOp = {
        1: "Online Banking",
        2: "Mobile Deposit",
        3: "Local Bank"
    };
    const StatusSelectOp = {
        1: "Pending",
        7: "On Hold",
        3: "Approved",
        4: "Rejected",
        5: "Cancelled"
    }

    class DepositList extends React.Component {
        constructor(props) {
            super(props);

        }

        onCheckDetail(id) {
            this.props.onCheckDetail(id);
        }

        onRefresh() {
            this.props.onRefresh();
        }

        onExport() {
            this.props.onExport();
        }

        render() {
            let rows = this.props.searchData.data.map(row => {
                let styleblod = (row.flag === 0) ? { fontWeight: 'bold' } : {};
                let showbankname = (row.depositType === 1) ? row.bankName + " ( " + row.payName + " ) " : (row.depositType === 2) ? row.channelName + " ( " + row.payName + " ) " : (row.depositType === 3) ? row.bankName : null;
                return (
                    <tr key={row.transId} style={styleblod}>
                        <th scope="row">
                            {(subFuncKeys.indexOf("f_dp_execute") > -1) ? <a href="#" role="button" onClick={this.onCheckDetail.bind(this, row.transId)}>{row.transId}</a> : row.transId}
                        </th>
                        {/* <td>{row.calCode}</td> */}
                        <td>{i18n["lbl_DepositType" + row.depositType]}</td>
                        <td>{row.isKyc === 1 ? "YES" : "NO"}</td>
                        <td>{row.isRisk === 1 ? "YES" : "NO"}</td>
                        <td><a href={"/CustManage/CustInfo?UserName=" + row.userName + "&custId=" + row.custId + "&TransId=" + row.transId} target="_blank">{row.userName}</a></td>
                        <td>{i18n["lbl_Currency" + row.currencyId]}</td>
                        <td className="text-right">{number_format(row.amount)}</td>
                        <td>{showbankname}</td>
                        <td>{row.accountNo}</td>
                        <td>{row.accountName}</td>
                        <td>{moment(row.createTime).format('l LTS')}</td>
                        <td>{row.updateTime ? moment(row.updateTime).format('l LTS') : null}</td>
                        <td className="text-right">{row.depositCnt}</td>
                        <td>{DepositStatusOp[row.status]}</td>
                    </tr>
                )
            }
            );

            let RefreshBtn = null;
            if (this.props.onRefresh == null) {
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
                        {RefreshBtn}<button type="button" className="btn btn-approved" onClick={this.onExport.bind(this)}>Export Excel</button>
                    </div>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">Trxn.ID</th>
                                {/* <th scope="col">Calvin Code</th> */}
                                <th scope="col">{i18n.lbl_DepositType}</th>
                                <th scope="col">Is Kyc</th>
                                <th scope="col">Is Risk</th>
                                <th scope="col">{i18n.lbl_UserName}</th>
                                <th scope="col">CCY</th>
                                <th scope="col" className="text-right">{i18n.lbl_Amount}</th>
                                <th scope="col">Bank Name</th>
                                <th scope="col">To Acc. NO.</th>
                                <th scope="col">To Acc. Name</th>
                                <th scope="col">{i18n.lbl_CreateTime + " (" + moment().format('Z') + ")"}</th>
                                <th scope="col">{"Update Time (" + moment().format('Z') + ")"}</th>
                                <th scope="col" className="text-right">Total Deposit Count</th>
                                <th scope="col">{i18n.lbl_Status}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <Pagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                </div>
            )
        }
    }

    class DepositRequestComment extends React.Component {
        constructor(props) {
            super(props);
        }
        render() {
            let data = this.props.Data;
            let i = 1;
            let rows = null;
            if (data != null) {
                rows = data.map(function (obj) {
                    return <tr key={i}>
                        <td scope="row">{i++}</td>
                        <td>{moment(obj.createTime).format('l LTS')}</td>
                        <td>{obj.comment}</td>
                        <td>{i18n["lbl_DepositStatus" + obj.action]}</td>
                        <td>{obj.operator}</td>
                    </tr>;
                });
            }

            return (
                data != null ?
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

    class DepositDetail extends React.Component {
        _clipboard = null;
        constructor(props) {
            super(props);
            this.state = { Data: null };
        }

        componentDidMount() {
            this.onGetData();
            this.onGetComment();
            this._clipboard = new ClipboardJS(".js-copy");

            this._clipboard.on('success', function (e) {
                //console.log(e);
            });
            this._clipboard.on('error', function (e) {
                //console.log(e);
            });
        }

        onGetData() {
            //Loading.OnLoading();
            axios.post('/api/Deposit/GetDepositRequest', { transId: this.props.transId })
                .then(async function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ Data: result.data, selectType: result.data.DepositType, GetData: true, Comment: result.data.comment });
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
            this.setState({ Comment: event.target.value });
        }

        onGetComment() {
            axios.post('/api/Deposit/GetDepositRequestComment', { transId: this.props.transId })
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ Comments: result.data, GetData: true });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
        }

        onConfirm(action) {
            let confirmstring = "Are you sure";
            switch (action) {
                case 0:
                    confirmstring += " Draft ?";
                    break;
                case 1:
                    confirmstring += " Pending ?";
                    break;
                case 3:
                    confirmstring += " Approved ?";
                    break;
                case 4:
                    confirmstring += " Rejected ?";
                    break;
                case 7:
                    confirmstring += " On Hold(Risk Check) ?";
                    break;
                default:
                    confirmstring += " ?";
                    break;
            }

            if ((action != 0 && confirm(confirmstring)) || action == 0) {
                Loading.OnLoading();
                let data = { transId: this.props.transId, Comment: this.state.Comment, Action: action, ActualDeposit: this.state.Data.amount };

                axios.post('/api/Deposit/DepositConfirm', data)
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            alert(result.message);
                            if (action != 0) {
                                this.onBack();
                            } else {
                                this.onGetComment();
                            }
                        } else {
                            alert(result.message);
                            this.onGetComment();
                        }
                    }.bind(this)).catch(function (error) {
                        Loading.UnLoading();
                        alert(error);
                    }.bind(this));
            }
        }

        render() {
            let data = this.state.Data;
            let btnGroup = [];
            let PayFrom = [];
            if (data != null) {
                let dS = DepositStatus;
                if (data.status == dS.Pending || data.status == dS.OnHold)
                    btnGroup.push(<button key={0} type="button" className="btn btn-draft" onClick={this.onConfirm.bind(this, 0)}>Draft</button>);
                if (data.status == dS.OnHold && data.depositType == 3)
                    btnGroup.push(<button key={1} type="button" className="btn btn-hold" onClick={this.onConfirm.bind(this, 1)}>Pending</button>);
                if (data.status == dS.Pending && data.depositType == 3)
                    btnGroup.push(<button key={7} type="button" className="btn btn-hold" onClick={this.onConfirm.bind(this, 7)}>On Hold(Risk Check)</button>);
                if (data.status == dS.Pending && data.depositType == 3)
                    btnGroup.push(<button key={3} type="button" className="btn btn-approved" onClick={this.onConfirm.bind(this, 3)}>Approved</button>);
                if (data.status == dS.Pending)
                    btnGroup.push(<button key={4} type="button" className="btn btn-rejected" onClick={this.onConfirm.bind(this, 4)}>Rejected</button>);
                let payK = 0;
                PayFrom.push(<tr key={payK++}>
                    <th scope="row">To Bank Code</th>
                    <td>{data.bankCode}</td>
                </tr>);

                PayFrom.push(<tr key={payK++}>
                    <th scope="row">To Bank Name</th>
                    <td>{data.bankName}</td>
                </tr>);

                if (data.payName != null) {
                    PayFrom.push(<tr key={payK++}>
                        <th scope="row">Payment</th>
                        <td>{data.payName}</td>
                    </tr>);
                }

                if (data.accountName != null) {

                    PayFrom.push(<tr key={payK++}>
                        <th scope="row">To Account Name</th>
                        <td>{data.accountName}<button type="button" className="btn btn-copy js-copy" data-clipboard-text={data.accountName}>Copy</button></td>
                    </tr>);
                    PayFrom.push(<tr key={payK++}>
                        <th scope="row">To Account No</th>
                        <td>{data.accountNo}<button type="button" className="btn btn-copy js-copy" data-clipboard-text={data.accountName}>Copy</button></td>
                    </tr>);
                }

                if (data.receipt) {
                    PayFrom.push(<tr key={payK++}>
                        <td colSpan={2}><img className="img-receipt" src={"/Finance/GetDepositReceiptPic?transId=" + data.transId} /></td>
                    </tr>);
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
                            {/* <tr>
                        <th scope="row">Calvin Code</th>
                        <td>{data.calCode}</td>
                    </tr>                                     */}
                            <tr>
                                <th scope="row">Status</th>
                                <td>{i18n["lbl_DepositStatus" + data.status]}</td>
                            </tr>
                            <tr>
                                <th scope="row">Is Kyc</th>
                                <td>{data.isKyc === 1 ? "YES" : "NO"}</td>
                            </tr>
                            <tr>
                                <th scope="row">Is Risk</th>
                                <td>{data.isRisk === 1 ? "YES" : "NO"}</td>
                            </tr>
                            <tr>
                                <th scope="row">Username</th>
                                <td><a href={"/CustManage/CustInfo?UserName=" + data.userName + "&custId=" + data.custId + "&TransId=" + data.transId} target="_blank">{data.userName}</a></td>
                            </tr>
                            <tr>
                                <th scope="row">Real Name</th>
                                <td>{data.realName}</td>
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
                                <th scope="row">Currency</th>
                                <td>{i18n["lbl_Currency" + data.currencyId]}</td>
                            </tr>
                            <tr>
                                <th scope="row">Request Amount</th>
                                <td>{number_format(data.amount)}<button type="button" className="btn btn-copy js-copy" data-clipboard-text={data.amount}>Copy</button></td>
                            </tr>
                            <tr>
                                <th scope="row">Deposit Type</th>
                                <td>{i18n["lbl_DepositType" + data.depositType]}</td>
                            </tr>
                            {PayFrom}
                            <tr>
                                <th scope="row">{"Deposit Time" + " (" + moment().format('Z') + ")"}</th>
                                <td>{moment(data.depositTime).format('l LTS')}</td>
                            </tr>
                            <tr>
                                <th scope="row">Ref ID</th>
                                <td>{data.refId}</td>
                            </tr>
                            <tr>
                                <th scope="row">Total Deposit Count</th>
                                <td>{data.depositCnt}</td>
                            </tr>
                            <tr>
                                <th scope="row">{"Update Time (" + moment().format('Z') + ")"}</th>
                                <td>{data.updateTime ? moment(data.updateTime).format('l LTS') : null}</td>
                            </tr>
                        </tbody>
                    </table>
                    <DepositRequestComment Data={this.state.Comments} />
                    <div className="input-field mt-20">
                        <label className="label" htmlFor="command">{i18n.lbl_Comment}</label>
                        <textarea id="command" className="form-control" rows="2" value={this.state.Comment} onChange={this.setComment.bind(this)} />
                    </div>
                    <div className="button-group mt-10">
                        <button type="button" className="btn btn-back" onClick={this.onBack.bind(this)}>Back</button>
                        <div className="pull-right">{btnGroup}</div>
                        <div className="clearfix"></div>
                    </div>
                </div> : null
            )
        }
    }

    class DepositPanel extends React.Component {
        _myTimer;
        _timeInterval = 15;
        constructor(props) {
            super(props);
            this.selectdate_start = React.createRef();
            this.selectdate_end = React.createRef();
            this.ExportForm = React.createRef();
            this.state = { SiteList: [], selectSite: [], selectType: [], selectCurrency: [], selectStatus: [], SelectUser: "", SelectDepositID: "", ShowDetial: false, timeInterval: this._timeInterval, IsRefresh: false, MutiSelRefresh: false };
            this.searchFilter = {};
        }

        onRefresh() {
            let searchData = this.state.searchData;
            this.onSearch(searchData.pageNumber);

        }

        onChange(event) {
            let obj = {};
            obj[event.target.name] = event.target.value;
            this.setState(obj);
        }

        onSearch(pageNumber) {
            this.setState({ IsRefresh: true });
            if (this._myTimer != null) {
                clearTimeout(this._myTimer);
            }
            //Loading.OnLoading();
            let data = { StartDate: this.searchFilter.StartDate, EndDate: this.searchFilter.EndDate, PageNumber: pageNumber, SelectSite: this.searchFilter.selectSite };

            if (this.searchFilter.SelectDepositID != "") {
                data.DepositId = this.searchFilter.SelectDepositID;
            }

            if (this.searchFilter.selectType.length != 0) {
                data.DepositType = this.searchFilter.selectType;
            }

            if (this.searchFilter.selectStatus.length != 0) {
                data.status = this.searchFilter.selectStatus;
            }

            if (this.searchFilter.SelectUser != "") {
                data.UserName = this.searchFilter.SelectUser;
            }

            if (this.searchFilter.selectCurrency.length != 0) {
                data.CurrencyID = this.searchFilter.selectCurrency;
            }
            axios.post('/api/Deposit/GetDepositRequestList', data)
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ searchData: result, GetData: true, IsRefresh: false, timeInterval: this._timeInterval });
                    } else {
                        alert(result.message);
                        this.setState({ IsRefresh: false, timeInterval: this._timeInterval });
                    }
                    this.AutoRefresh();
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();

                    alert(error);
                    this.setState({ IsRefresh: false, timeInterval: this._timeInterval });
                    this.AutoRefresh();
                }.bind(this));
        }

        onExport() {
            let data = { StartDate: this.searchFilter.StartDate, EndDate: this.searchFilter.EndDate, SelectSite: this.searchFilter.selectSite };
            if (this.searchFilter.SelectDepositID != "") {
                data.DepositId = this.searchFilter.SelectDepositID;
            }

            if (this.searchFilter.selectType.length != 0) {
                data.DepositType = this.searchFilter.selectType;
            }

            if (this.searchFilter.selectStatus.length != 0) {
                data.status = this.searchFilter.selectStatus;
            }

            if (this.searchFilter.SelectUser != "") {
                data.UserName = this.searchFilter.SelectUser;
            }

            if (this.searchFilter.selectCurrency.length != 0) {
                data.CurrencyID = this.searchFilter.selectCurrency;
            }
            let ExportParam = [];
            for (var prop in data) {
                ExportParam.push(<input key={prop} type="hidden" name={prop} value={data[prop]} />);
            }
            ExportParam = <form method="post" action={'/Finance/ExportDepositList'} ref={this.ExportForm}>{ExportParam}</form>;

            this.setState({ ExportParam, ExportParam }, function () {
                this.ExportForm.current.submit();
            }.bind(this));

            //axios.post('/Finance/ExportDepositList', data, {responseType: 'blob'});
        }

        handleSubmit(event) {
            event.preventDefault();
            if (this.state.selectSite.length == 0) {
                alert("Please Choose at least one Site")
                return;
            }

            if (this.state.selectType.length == 0) {
                alert("Please Choose at least one Type")
                return;
            }

            if (this.state.selectCurrency.length == 0) {
                alert("Please Choose at least one Currency")
                return;
            }

            if (this.state.selectStatus.length == 0) {
                alert("Please Choose at least one Status")
                return;
            }

            if (this.selectdate_start.current.state.inputValue == "") {
                alert("Please Choose From Date")
                return;
            }

            if (this.selectdate_end.current.state.inputValue == "") {
                alert("Please Choose To Date")
                return;
            }
            console.log(this.state);
            //this._Search = true;
            let data = { StartDate: moment(this.selectdate_start.current.state.inputValue).unix(), EndDate: moment(this.selectdate_end.current.state.inputValue).add(1, 'days').unix() - 1 };
            this.setState(data, function () {
                this.searchFilter = JSON.parse(JSON.stringify(this.state));
                this.onSearch(1);
            }.bind(this));
        }

        AutoRefresh() {
            if (this.state.timeInterval == 0) {
                this.onRefresh();
            } else {
                let timeInterval = this.state.timeInterval;
                timeInterval--;
                this.setState({ timeInterval: timeInterval });
                this._myTimer = setTimeout(function () {
                    this.AutoRefresh();
                }.bind(this), 1000);
            }
        }

        getSiteList() {
            let state = this.state;
            //Loading.OnLoading();
            axios.post('/api/Common/AdminUserSiteList')
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        state.selectSite.length = 0;
                        this.setState({ SiteList: result.data });
                        this.buildSelectSite();

                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
        }

        buildSelectSite = () => {
            let state = this.state, searchFilter = this.searchFilter;
            $("#selectSite").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    state.selectSite.length = 0;
                    state.SiteList.forEach(function (obj) {
                        state.selectSite.push(obj.siteId);
                    });
                },
                onDeselectAll: function () {
                    state.selectSite.length = 0;
                },
                onChange: function (option, checked, select) {
                    let val = parseInt($(option).val()), index = state.selectSite.indexOf(val);
                    (checked) ? state.selectSite.push(val) : (index > -1) ? state.selectSite.splice(index, 1) : state.selectSite;
                    state.selectSite.sort();
                },
                onInitialized: function () {
                    state.selectSite.length = 0;
                    (searchFilter.selectSite) ?
                        searchFilter.selectSite.forEach(function (obj) {
                            state.selectSite.push(parseInt(obj));
                        })
                        : state.SiteList.forEach(function (obj) {
                            state.selectSite.push(parseInt(obj));
                        });
                }
            });
            (state.MutiSelRefresh) ? $('#selectSite').multiselect('select', searchFilter.selectSite) : $('#selectSite').multiselect('selectAll', false);
            $('#selectSite').multiselect('updateButtonText');
        }

        buildTypeSite = () => {
            let state = this.state, searchFilter = this.searchFilter;
            $("#selectType").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    state.selectType.length = 0;
                    Object.keys(TypeSelectOp).forEach(function (obj) {
                        state.selectType.push(parseInt(obj));
                    });
                },
                onDeselectAll: function () {
                    state.selectType.length = 0;
                },
                onChange: function (option, checked, select) {
                    let val = parseInt($(option).val()), index = state.selectType.indexOf(val);
                    (checked) ? state.selectType.push(val) : (index > -1) ? state.selectType.splice(index, 1) : state.selectType;
                    state.selectType.sort();
                },
                onInitialized: function () {
                    state.selectType.length = 0;
                    (searchFilter.selectType) ?
                        searchFilter.selectType.forEach(function (obj) {
                            state.selectType.push(parseInt(obj));
                        })
                        : Object.keys(TypeSelectOp).forEach(function (obj) {
                            state.selectType.push(parseInt(obj));
                        });
                }
            });
            (state.MutiSelRefresh) ? $('#selectType').multiselect('select', searchFilter.selectType) : $('#selectType').multiselect('selectAll', false);
            $('#selectType').multiselect('updateButtonText');
        }

        buildCurrencySite = () => {
            let state = this.state, searchFilter = this.searchFilter;
            $("#selectCurrency").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    state.selectCurrency.length = 0;
                    CurrencyList.forEach(function (obj) {
                        state.selectCurrency.push(parseInt(obj));
                    });
                },
                onDeselectAll: function () {
                    state.selectCurrency.length = 0;
                },
                onChange: function (option, checked, select) {
                    let val = parseInt($(option).val()), index = state.selectCurrency.indexOf(val);
                    (checked) ? state.selectCurrency.push(val) : (index > -1) ? state.selectCurrency.splice(index, 1) : state.selectCurrency;
                    state.selectCurrency.sort();
                },
                onInitialized: function () {
                    state.selectCurrency.length = 0;
                    (searchFilter.selectCurrency) ?
                        searchFilter.selectCurrency.forEach(function (obj) {
                            state.selectCurrency.push(parseInt(obj));
                        })
                        : CurrencyList.forEach(function (obj) {
                            state.selectCurrency.push(parseInt(obj));
                        });
                }
            });
            (state.MutiSelRefresh) ? $('#selectCurrency').multiselect('select', searchFilter.selectCurrency) : $('#selectCurrency').multiselect('selectAll', false);
            $('#selectCurrency').multiselect('updateButtonText');
        }

        buildStatusSite = () => {
            let state = this.state, searchFilter = this.searchFilter;
            $("#selectStatus").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    state.selectStatus.length = 0;
                    Object.keys(StatusSelectOp).forEach(function (obj) {
                        state.selectStatus.push(parseInt(obj));
                    });
                },
                onDeselectAll: function () {
                    state.selectStatus.length = 0;
                },
                onChange: function (option, checked, select) {
                    let val = parseInt($(option).val()), index = state.selectStatus.indexOf(val);
                    (checked) ? state.selectStatus.push(val) : (index > -1) ? state.selectStatus.splice(index, 1) : state.selectStatus;
                    state.selectStatus.sort();
                },
                onInitialized: function () {
                    state.selectStatus.length = 0;
                    (searchFilter.selectStatus) ?
                        searchFilter.selectStatus.forEach(function (obj) {
                            state.selectStatus.push(parseInt(obj));
                        })
                        : state.selectStatus.push(1);
                }
            });
            (state.MutiSelRefresh) ? $('#selectStatus').multiselect('select', searchFilter.selectStatus) : $('#selectStatus').multiselect('select', 1);
            $('#selectStatus').multiselect('updateButtonText');

        }

        componentDidUpdate() {
            console.log("didup");
            if (this.state.MutiSelRefresh) {
                console.log("didup1");
                this.buildSelectSite();
                this.buildTypeSite();
                this.buildCurrencySite();
                this.buildStatusSite();
                this.setState({ MutiSelRefresh: false });
            }
        }

        componentDidMount() {
            console.log("didmon");
            this.getSiteList();
            this.buildTypeSite();
            this.buildCurrencySite();
            this.buildStatusSite();
        }

        onCheckDetail(id) {
            this.setState({ transId: id, ShowDetial: true });
        }

        onBack() {
            this.setState({ ShowDetial: false, MutiSelRefresh: true });
            this.onRefresh();
        }

        render() {
            let siteList = this.state.SiteList.map(function (obj) {
                return <option key={`site-list-${obj.siteId}`} value={obj.siteId}>{obj.siteName}</option>;
            }.bind(this));
            siteList = <select id="selectSite" name="selectSite[]" multiple="multiple">{siteList}</select>;

            let typeList = Object.keys(TypeSelectOp).map(function (obj) {
                return <option key={`type-list-${obj}`} value={obj}>{TypeSelectOp[obj]}</option>;
            }.bind(this));
            typeList = <select id="selectType" name="selectType[]" multiple="multiple">{typeList}</select>;

            let currencyList = CurrencyList.map(function (obj) {
                return <option key={`currency-list-${obj}`} value={obj}>{i18n["lbl_Currency" + obj]}</option>;
            }.bind(this));
            currencyList = <select id="selectCurrency" name="selectCurrency[]" multiple="multiple">{currencyList}</select>;

            let statusList = Object.keys(StatusSelectOp).map(function (obj) {
                return <option key={`status-list-${obj}`} value={obj}>{StatusSelectOp[obj]}</option>;
            }.bind(this));
            statusList = <select id="selectStatus" name="selectStatus[]" multiple="multiple">{statusList}</select>;

            let searchData = this.state.searchData;
            let StartDate = this.state.StartDate;
            let EndDate = this.state.EndDate;

            return (
                <div>
                    {!this.state.ShowDetial ? <form method="post" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="filter">
                            <div className="row-flex">
                                <div className="input-item">
                                    <span className="label">Site</span>
                                    <div className="field">
                                        {siteList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Type</span>
                                    <div className="field">
                                        {typeList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">From Date</span>
                                    <div className="date">
                                        <DateTimePicker ref={this.selectdate_start} timeFormat={false} defaultValue={StartDate != null ? moment.unix(StartDate).format("l") : moment().add(-10, 'days').format('l')} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">To Date</span>
                                    <div className="date">
                                        <DateTimePicker ref={this.selectdate_end} timeFormat={false} defaultValue={EndDate != null ? moment.unix(EndDate).format("l") : moment().format('l')} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Currency</span>
                                    <div className="field">
                                        {currencyList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Status</span>
                                    <div className="field">
                                        {statusList}
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
                                    <span className="label">Deposit ID</span>
                                    <div className="field">
                                        <input type="text" name="SelectDepositID" value={this.state.SelectDepositID} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>

                                <div className="button-group">
                                    <button className="btn btn-search" type="submit"><span>Search</span></button>
                                </div>
                            </div>
                        </div>
                    </form> : null}
                    {searchData != null && !this.state.ShowDetial ? <DepositList onExport={this.onExport.bind(this)} timeInterval={this.state.timeInterval} searchData={searchData} ChangePage={this.onSearch.bind(this)} onCheckDetail={this.onCheckDetail.bind(this)} onRefresh={!this.state.IsRefresh ? this.onRefresh.bind(this) : null} /> : null}
                    {this.state.transId != null && this.state.ShowDetial ? <DepositDetail transId={this.state.transId} onBack={this.onBack.bind(this)} /> : null}
                    <div style={{ display: 'none' }}>
                        {this.state.ExportParam}
                    </div>
                </div>
            )
        }
    }
    ReactDOM.render(<DepositPanel />, document.getElementById('searchForm'));
})(jQuery, React, ReactDOM, i18n, axios, moment);