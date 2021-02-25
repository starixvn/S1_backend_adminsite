import { Pagination } from '../commJS/Pagination.jsx';
import * as Loading from '../commJS/Loading.jsx';

(function ($, React, ReactDOM, i18n, axios, moment) {
    const CustStatusOp = {
        1: <span className="status status-active">Running</span>,
        2: <span className="status status-suspend">Suspend</span>,
        3: <span className="status status-closed">Closed</span>,
    }

    const defCurrencyType = {
        2: "MYR",
        4: "THB",
        13: "RMB",
        15: "IDR",
        51: "VND",
        96: "USDT",
    };

    class CustInfoRow extends React.Component {
        render() {
            let data = this.props.dataRow;
            return (
                <tr key={data.custId}>
                    <td>{data.siteName}</td>
                    <td><a href={"/CustManage/CustInfo?UserName=" + data.userName + "&custId=" + data.custId + "&TransId=" + (this.props.transId || "")} target="_blank">{data.userName}</a></td>
                    {
                        (data.siteName == "Nova88") ?
                            <td>{data.levelName}</td>
                            :
                            <td>{data.userLevel}</td>
                    }
                    <td>{i18n["lbl_Currency" + data.currencyId]}</td>
                    <td>{data.agent}</td>
                    <td>{data.isTest.toString()}</td>
                    <td>{moment(data.regTime).format('l LTS')}</td>
                    <td>{data.sabaID}</td>
                    <td>{CustStatusOp[data.status]}</td>
                </tr>
            )
        }
    }

    class CustInfoList extends React.Component {
        onExport() {
            this.props.onExport();
        }

        render() {
            let data = this.props.searchData && this.props.searchData.data;
            let custInfoRows = null;
            let TransId = this.props.TransId;
            if (data != null) {
                custInfoRows = <tbody>{data.map(x => <CustInfoRow transId={TransId} key={x.custId} dataRow={x} />)}</tbody>;
            }

            return (
                data.Loading ?
                    <div className="text-center">
                        <div className="spinner-border" role="status">
                            <span className="sr-only">Loading...</span>
                        </div>
                    </div>
                    :
                    <div className="table-responsive-md table">
                        {subFuncKeys.indexOf("ca_exportExcel") > -1 ?
                            < div className="table-options">
                                <button type="button" className="btn btn-approved" onClick={this.onExport.bind(this)}>Export Excel</button>
                            </div>
                            : null
                        }
                        <table className="table table-striped">
                            <thead>
                                <tr>
                                    <th scope="col">Site Name</th>
                                    <th scope="col">User Name</th>
                                    <th scope="col">Vip Level</th>
                                    <th scope="col">Currency</th>
                                    <th scope="col">Agent</th>
                                    <th scope="col">IsTest</th>
                                    <th scope="col">Registration time</th>
                                    <th scope="col">Saba ID</th>
                                    <th scope="col">Status</th>
                                </tr>
                            </thead>
                            {custInfoRows}
                        </table>
                        <Pagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                    </div >
            )
        }
    }

    class CustInfoPanel extends React.Component {
        _PageSize = 10;
        _Search = false;
        constructor(props) {
            super(props);
            this.state = { SiteList: [], selectSite: [], searchData: null, UserName: "", TransId: "", Loading: false, CurrencyList: [], selectCurrency: [] };
            this.f_trigger_Start = React.createRef();
            this.selectdate_start = React.createRef();
            this.f_trigger_End = React.createRef();
            this.selectdate_end = React.createRef();
            this.ExportForm = React.createRef();
        }

        handleSubmit(event) {
            event.preventDefault();
            if ((this.state.BirDate || this.state.BirMonth) && (!this.state.BirDate || !this.state.BirMonth)) { alert("Please fill in at least Date and Month"); return; }
            let endDate = moment(this.selectdate_end.current.value).add(1, 'days').unix() - 1;
            this._Search = true;
            this.setState({
                StartDate: moment(this.selectdate_start.current.value).unix(),
                EndDate: endDate,
                sBirDate: this.state.BirDate,
                sBirMonth: this.state.BirMonth,
                sBirYear: this.state.BirYear,
                sUserName: this.state.UserName,
                sTransId: this.state.TransId,
                sTpUserName: this.state.TpUserName,
                sAffId: this.state.AffId,
                sAccountName: this.state.AccountName,
                sLoginIp: this.state.LoginIp,
                sEmail: this.state.Email,
                sPhoneNo: this.state.PhoneNo,
                Loading: true
            }, function () {
                this.onSearch(1);
            }.bind(this));
        }

        onSearch(pageNumber) {
            Loading.OnLoading();
            let data = {
                SelectSite: !this.state.selectSite ? null : this.state.selectSite,
                StartDate: !this.state.StartDate ? null : this.state.StartDate,
                EndDate: !this.state.EndDate ? null : this.state.EndDate,
                BirDate: !this.state.sBirDate ? null : parseInt(this.state.sBirDate),
                BirMonth: !this.state.sBirMonth ? null : parseInt(this.state.sBirMonth),
                BirYear: !this.state.sBirYear ? null : parseInt(this.state.sBirYear),
                UserName: !this.state.sUserName ? null : this.state.sUserName,
                TransId: !this.state.sTransId ? null : this.state.sTransId,
                TpUserName: !this.state.sTpUserName ? null : this.state.sTpUserName,
                AccountName: !this.state.sAccountName ? null : this.state.sAccountName,
                LoginIp: !this.state.sLoginIp ? null : this.state.sLoginIp,
                Email: !this.state.sEmail ? null : this.state.sEmail,
                PhoneNo: !this.state.sPhoneNo ? null : this.state.sPhoneNo,
                PageNumber: pageNumber,
                PageSize: this._PageSize,
                AffId: !this.state.sAffId ? null : parseInt(this.state.sAffId),
                SelectCurrency: !this.state.selectCurrency ? null : this.state.selectCurrency
            };
            axios.post('/api/CustManage/GetCustInfoList', data)
                .then(function (response) {
                    Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ searchData: result, Loading: false });
                    } else {
                        alert(result.message);
                        this.setState({ Loading: false });
                    }
                }.bind(this)).catch(function (error) {
                    Loading.UnLoading();
                    alert(error);
                    this.setState({ Loading: false });
                }.bind(this));
        }

        onExport() {
            let data = {
                SelectSite: !this.state.selectSite ? null : this.state.selectSite,
                StartDate: !this.state.StartDate ? null : this.state.StartDate,
                EndDate: !this.state.EndDate ? null : this.state.EndDate,
                BirDate: !this.state.sBirDate ? null : parseInt(this.state.sBirDate),
                BirMonth: !this.state.sBirMonth ? null : parseInt(this.state.sBirMonth),
                BirYear: !this.state.sBirYear ? null : parseInt(this.state.sBirYear),
                UserName: !this.state.sUserName ? null : this.state.sUserName,
                TransId: !this.state.sTransId ? null : this.state.sTransId,
                TpUserName: !this.state.sTpUserName ? null : this.state.sTpUserName,
                AccountName: !this.state.sAccountName ? null : this.state.sAccountName,
                LoginIp: !this.state.sLoginIp ? null : this.state.sLoginIp,
                Email: !this.state.sEmail ? null : this.state.sEmail,
                PhoneNo: !this.state.sPhoneNo ? null : this.state.sPhoneNo,
                AffId: !this.state.sAffId ? null : parseInt(this.state.sAffId)
            };

            let ExportParam = [];
            for (var prop in data) {
                ExportParam.push(<input key={prop} type="hidden" name={prop} value={data[prop]} />);
            }

            ExportParam = <form method="post" action={'/CustManage/ExportCustInfoList'} ref={this.ExportForm}>{ExportParam}</form>;

            this.setState({ ExportParam, ExportParam }, function () {
                this.ExportForm.current.submit();
            }.bind(this));

            //axios.post('/Finance/ExportDepositList', data, {responseType: 'blob'});
        }
        onUserNameChange(e) {
            this.setState({ UserName: e.target.value });
        }

        onTransIdChange(e) {
            this.setState({ TransId: e.target.value });
        }

        onChange(event) {
            let obj = {};
            obj[event.target.name] = event.target.value;
            this.setState(obj);
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
            let state = this.state;
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
                }
            });
            $('#selectSite').multiselect('updateButtonText');
        }

        getCurrencyList() {
            let state = this.state;
            this.buildSelectCurrency();

        }

        buildSelectCurrency = () => {
            let state = this.state;
            $("#selectCurrency").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    console.log(state.selectCurrency);
                    state.selectCurrency.length = 0;
                    console.log(state.selectCurrency);
                    // state.SiteList.forEach(function (obj) {
                    //     state.selectSite.push(obj.siteId);
                    // });
                    Object.keys(defCurrencyType).forEach((key) => {
                        state.selectCurrency.push(parseInt(key));
                    })
                },
                onDeselectAll: function () {
                    state.selectCurrency.length = 0;
                },
                onChange: function (option, checked, select) {
                    let val = parseInt($(option).val()), index = state.selectCurrency.indexOf(val);
                    (checked) ? state.selectCurrency.push(val) : (index > -1) ? state.selectCurrency.splice(index, 1) : state.selectCurrency;
                    state.selectCurrency.sort();
                }
            });
            $('#selectCurrency').multiselect('updateButtonText');
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
            this.getSiteList();
            this.getCurrencyList();
        }

        render() {
            let inputStyle = {
                marginRight: "10px",
                width: 'calc(100% / 3 - 5px)',
                fallbacks: [
                    { width: '-moz-calc(100% / 3 - 5px)' },
                    { width: '-webkit-calc(100% / 3 - 5px)' },
                    { width: '-o-calc(100% / 3 - 5px)' }
                ]
            }

            let inputStyle2 = {
                width: 'calc(100% / 3 - 10px)',
                fallbacks: [
                    { width: '-moz-calc(100% / 3 - 10px)' },
                    { width: '-webkit-calc(100% / 3 - 10px)' },
                    { width: '-o-calc(100% / 3 - 10px)' }
                ]
            }
            let StartDate = this.state.StartDate;
            let EndDate = this.state.EndDate;
            let siteList = this.state.SiteList.map(function (obj) {
                return <option key={obj.siteId} value={obj.siteId}>{obj.siteName}</option>;
            }.bind(this));
            let currencyList = Object.keys(defCurrencyType).map((key) => {
                return <option key={key} value={key}>{defCurrencyType[key]}</option>;
            });
            siteList = <select id="selectSite" name="selectSite[]" multiple="multiple">{siteList}</select>;
            currencyList = <select id="selectCurrency" name="selectCurrency[]" multiple="multiple">{currencyList}</select>;
            return (
                <div>
                    <h4 className="page-name">Customer Search</h4>
                    <form onSubmit={this.state.Loading ? null : this.handleSubmit.bind(this)}>
                        <div className="filter">
                            <div className="row-flex">
                                <div className="input-item">
                                    <span className="label">Site</span>
                                    <div className="field">
                                        {siteList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">User Name</label>
                                    <div className="field">
                                        <input type="text" placeholder="User Name" autoComplete="off" value={this.state.UserName} onChange={this.onUserNameChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Currency</span>
                                    <div className="field">
                                        {currencyList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Real Name</label>
                                    <div className="field">
                                        <input type="text" placeholder="Real Name" autoComplete="off" name="AccountName" value={this.state.AccountName} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Saba ID</label>
                                    <div className="field">
                                        <input type="text" placeholder="Saba ID" autoComplete="off" name="TpUserName" value={this.state.TpUserName} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Affiliates ID</label>
                                    <div className="field">
                                        <input type="text" placeholder="Affiliates ID" autoComplete="off" name="AffId" value={this.state.AffId} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Login Ip</label>
                                    <div className="field">
                                        <input type="text" placeholder="Login Ip" autoComplete="off" name="LoginIp" value={this.state.LoginIp} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Transaction ID</label>
                                    <div className="field">
                                        <input type="text" placeholder="Transaction ID" autoComplete="off" value={this.state.TransId} onChange={this.onTransIdChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">E-Mail</label>
                                    <div className="field">
                                        <input type="text" placeholder="E-Mail" autoComplete="off" name="Email" value={this.state.Email} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Phone No</label>
                                    <div className="field">
                                        <input type="text" placeholder="Phone No" autoComplete="off" name="PhoneNo" value={this.state.PhoneNo} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Birthday</label>
                                    <div className="field" style={{ display: 'inline-flex' }}>
                                        <input type="text" placeholder="Date" maxLength="2" autoComplete="off" name="BirDate" value={this.state.BirDate} style={inputStyle} onChange={this.onChange.bind(this)} />
                                        <input type="text" placeholder="Month" maxLength="2" autoComplete="off" name="BirMonth" value={this.state.BirMonth} style={inputStyle} onChange={this.onChange.bind(this)} />
                                        <input type="text" placeholder="Year" maxLength="4" autoComplete="off" name="BirYear" value={this.state.BirYear} style={inputStyle2} onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Register Date (From)</span>
                                    <div className="date">
                                        <input ref={this.selectdate_start} type="text" defaultValue={!isNaN(StartDate) ? moment.unix(StartDate).format('l') : null} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_Start}>{/*<img ref={this.f_trigger_Start} src="/css/Oneworks/images/icon-calendar.png" />*/}</a>
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Register Date (To)</span>
                                    <div className="date">
                                        <input ref={this.selectdate_end} type="text" defaultValue={!isNaN(EndDate) ? moment.unix(EndDate).format('l') : null} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_End}>{/*<img ref={this.f_trigger_End} src="/css/Oneworks/images/icon-calendar.png" />*/}</a>
                                    </div>
                                </div>
                                <div className="button-group">
                                    <button type="submit" className="btn btn-primary">Search</button>
                                </div>
                            </div>
                        </div>
                    </form>
                    {this._Search && this.state.searchData != null ? <CustInfoList onExport={this.onExport.bind(this)} searchData={this.state.searchData} TransId={this.state.sTransId} ChangePage={this.onSearch.bind(this)} /> : null}
                    <div style={{ display: 'none' }}>
                        {this.state.ExportParam}
                    </div>
                </div>
            )
        }
    }

    ReactDOM.render(<CustInfoPanel />, document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios, moment);