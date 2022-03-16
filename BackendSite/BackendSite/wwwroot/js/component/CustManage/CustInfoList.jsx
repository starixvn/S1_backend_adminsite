import { NewPagination } from '../commJS/NewPagination.jsx';
import * as Loading from '../commJS/Loading.jsx';
(function ($, React, ReactDOM, i18n, axios, moment) {
    const CustStatusOp = {
        1: <span className="status status-active">Running</span>,
        2: <span className="status status-suspend">Suspend</span>,
        3: <span className="status status-closed">Closed</span>,
        4: <span className="status status-lock">Lock</span>
    }

    const defCurrencyType = {
        2: "MYR",
        4: "THB",
        13: "RMB",
        15: "IDR",
        51: "VND",
        61: "INR",
    };
    const defStatusType = {
        1: "Running",
        2: "Suspend",
        3: "Closed",
        4: "Lock",
    };
    const defExportTransferWalletType = {
        0: "Close",
        1: "Open"
    };

    class CustInfoRow extends React.Component {
        render() {
            let data = this.props.dataRow;
            let tagData = this.props.tagList;
            let tagList = [];
            if(data.iTagList.length > 0 &&　tagData !== null){
                for(let i=0; i < data.iTagList.length;i++){
                    tagData.forEach(element=>{
                        if(element.id === data.iTagList[i]){
                            tagList.push({
                                tagName: element.tagName,
                                tagColour: element.tagColour
                            });
                        }
                    })
                }
            }
            return (
                <tr key={data.custId}>
                    <td>{data.siteName}</td>
                    <td><a href={"/CustManage/CustInfo?UserName=" + data.userName + "&custId=" + data.custId + "&TransId=" + (this.props.transId || "")} target="_blank">{data.userName}</a></td>
                    <td>{data.sabaID}</td> 
                    <td>{data.affiliateCode}</td>
                    <td>{tagList.map(x => <TagComponent tagInfo={x} />)}</td>
                    <td>{data.levelName}</td>
                    <td>{data.userLevel}</td>
                    <td>{i18n["lbl_Currency" + data.currencyId]}</td>
                    <td>{data.agent}</td>
                    <td>{data.isTest.toString()}</td>
                    <td>{data.balance}</td>
                    <td>{data.bonus == 0 ? "": data.bonus}</td>
                    <td>{moment.utc(data.regTime).add(8,'hour').format('l LTS')}</td>
                    <td>{data.reloadTime === null ? 'N/A' : moment.utc(data.reloadTime).add(8,'hour').format('l LTS')}</td>
                    <td>{data.lastTagUpdateTime === null ? 'N/A' : moment.utc(data.lastTagUpdateTime).add(8,'hour').format('l LTS')}</td>
                    <td>{data.lastStatusUpdateTime === null ? 'N/A' : moment.utc(data.lastStatusUpdateTime).add(8,'hour').format('l LTS')}</td>                 
                    <td>{CustStatusOp[data.status]}</td>
                    <td>{data.ip}</td>  
                </tr>
            )
        }
    }

    class TagComponent extends React.Component {
        render() {
            let data = this.props.tagInfo;
            return (
                <div>
                    <span style={{'padding-right': '3px'}}>{data.tagName}</span>
                    <span style={{ 'background-color': data.tagColour }}>　</span>
                </div>
            )
        }
    }

    class CustInfoList extends React.Component {
        onExport() {
            this.props.onExport();
        }

        render() {
            let data = this.props.searchData && this.props.searchData.data;
            let tagList = this.props.TagList;
            let custInfoRows = null;
            let TransId = this.props.TransId;
            if (data != null) {
                custInfoRows = <tbody>{data.map(x => <CustInfoRow transId={TransId} key={x.custId} dataRow={x} tagList={tagList}/>)}</tbody>;
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
                                    <th scope="col">Saba ID</th>  
                                    <th scope="col">Affiliates Code</th>
                                    <th scope="col">Tag</th>
                                    <th scope="col">Vip Level</th>
                                    <th scope="col">Deposit Limit Level</th>
                                    <th scope="col">Currency</th>
                                    <th scope="col">Agent</th>
                                    <th scope="col">IsTest</th>
                                    <th scope="col">Seamless Wallet</th>
                                    <th scope="col">Bonus Wallet</th>
                                    <th scope="col">
                                        <tr>Registration time</tr>
                                        <tr>(Adjusted to GMT+8)</tr>                                      
                                    </th>
                                    <th scope="col"> 
                                        <tr>Last Active Time</tr>
                                        <tr>(Adjusted to GMT+8)</tr>
                                    </th>
                                    <th scope="col"> 
                                        <tr>Last Tag Update Time</tr>
                                        <tr>(Adjusted to GMT+8)</tr>
                                    </th>
                                    <th scope="col"> 
                                        <tr>Last Status Update Time</tr>
                                        <tr>(Adjusted to GMT+8)</tr>
                                    </th>           
                                    <th scope="col">Status</th>
                                    <th scope="col">Last Activity IP</th>
                                </tr>
                            </thead>
                            {custInfoRows}
                        </table>
                        <NewPagination Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                    </div >
            )
        }
    }

    class CustInfoPanel extends React.Component {
        _PageSize = 10;
        _Search = false;
        constructor(props) {
            super(props);
            this.state = { SiteList: [], selectSite: [], searchData: null, UserName: "", TransId: "", Loading: false, CurrencyList: [], selectCurrency: [] , selectStatus:[], TagList:[] ,selectTag: [],ExportTransferWallet:0};
            this.f_trigger_Start = React.createRef();
            this.selectdate_start = React.createRef();
            this.f_trigger_End = React.createRef();
            this.selectdate_end = React.createRef();
            
            this.f_trigger_TagStart = React.createRef();
            this.selectTagDate_start = React.createRef();
            this.f_trigger_TagEnd = React.createRef();
            this.selectTagDate_end = React.createRef();

            this.f_trigger_StatusStart = React.createRef();
            this.selectStatusDate_start = React.createRef();
            this.f_trigger_StatusEnd = React.createRef();
            this.selectStatusDate_end = React.createRef();
            
            this.ExportForm = React.createRef();
        }

        handleSubmit(event) {
            event.preventDefault();
            if ((this.state.BirDate || this.state.BirMonth) && (!this.state.BirDate || !this.state.BirMonth)) { alert("Please fill in at least Date and Month"); return; }
            let endDate = moment(this.selectdate_end.current.value).add(1, 'days').unix() - 1;
            let tagEndDate = moment(this.selectTagDate_end.current.value).add(1, 'days').unix() - 1;
            let statusEndDate = moment(this.selectStatusDate_end.current.value).add(1, 'days').unix() - 1;
            this._Search = true;
            this.setState({
                StartDate: moment(this.selectdate_start.current.value).unix(),
                EndDate: endDate,
                TagStartDate: moment(this.selectTagDate_start.current.value).unix(),
                TagEndDate: tagEndDate,
                StatusStartDate: moment(this.selectStatusDate_start.current.value).unix(),
                StatusEndDate: statusEndDate,
                sBirDate: this.state.BirDate,
                sBirMonth: this.state.BirMonth,
                sBirYear: this.state.BirYear,
                sUserName: this.state.UserName,
                sTransId: this.state.TransId,
                sTpUserName: this.state.TpUserName,
                sAffCode: this.state.AffCode,
                sAccountName: this.state.AccountName,
                sLoginIp: this.state.LoginIp,
                sEmail: this.state.Email,
                sPhoneNo: this.state.PhoneNo,
                Loading: true,
                sBankAccountNumber: this.state.BankAccountNumber
            }, function () {
                this.onSearch(1);
            }.bind(this));
        }

        onSearch(pageNumber) {
            Loading.OnLoading();
            this.setState({ Loading: true });
            let data = {
                SelectSite: !this.state.selectSite ? null : this.state.selectSite,
                StartDate: !this.state.StartDate ? null : this.state.StartDate,
                EndDate: !this.state.EndDate ? null : this.state.EndDate,
                TagStartDate: !this.state.TagStartDate ? null : this.state.TagStartDate,
                TagEndDate: !this.state.TagEndDate ? null : this.state.TagEndDate,
                StatusStartDate: !this.state.StatusStartDate ? null : this.state.StatusStartDate,
                StatusEndDate: !this.state.StatusEndDate ? null : this.state.StatusEndDate,
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
                AffCode: !this.state.sAffCode ? null : this.state.sAffCode,
                SelectCurrency: !this.state.selectCurrency ? null : this.state.selectCurrency,
                BankAccountNumber: !this.state.sBankAccountNumber ? null : this.state.sBankAccountNumber,
                SelectStatus:!this.state.selectStatus ? null : this.state.selectStatus,
                selectTag:!this.state.selectTag ? null : this.state.selectTag
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
                    Lading.UnLoading();
                    alert(error);
                    this.setState({ Loading: false });
                }.bind(this));
        }

        onExport() {
            let data = {
                SelectSite: !this.state.selectSite ? null : this.state.selectSite,
                StartDate: !this.state.StartDate ? null : this.state.StartDate,
                EndDate: !this.state.EndDate ? null : this.state.EndDate,
                TagStartDate: !this.state.TagStartDate ? null : this.state.TagStartDate,
                TagEndDate: !this.state.TagEndDate ? null : this.state.TagEndDate,
                StatusStartDate: !this.state.StatusStartDate ? null : this.state.StatusStartDate,
                StatusEndDate: !this.state.StatusEndDate ? null : this.state.StatusEndDate,
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
                AffCode: !this.state.sAffCode ? null : this.state.sAffCode,
                SelectCurrency: !this.state.selectCurrency ? null : this.state.selectCurrency,
                BankAccountNumber: !this.state.sBankAccountNumber ? null : this.state.sBankAccountNumber,
                SelectStatus:!this.state.selectStatus ? null : this.state.selectStatus,
                selectTag:!this.state.selectTag ? null : this.state.selectTag,
                ExportTransferWallet: this.state.ExportTransferWallet == 0 ? false:true
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

        GetAllTagData(){
            axios.post('/api/CustManage/GetAllTag')
                    .then(function (response) {
                        let result = response.data;
                        if (result.errorCode == 0) {
                            this.setState({...this.state, TagList:result.data});
                            this.buildSelectTag();
                        } else {
                            alert(result.message);
                        }
                    }.bind(this)).catch(function (error) {
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
                },
                onInitialized: function () {
                    state.SiteList.forEach(function (obj) {
                        state.selectSite.push(obj.siteId);
                    });
                }
            });
            $('#selectSite').multiselect('updateButtonText');
        }

        buildSelectTag = () => {
            let state = this.state;
            $("#selectTag").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    state.selectTag.length = 0;
                    state.TagList.forEach(function (obj) {
                        state.selectTag.push(obj.id);
                    });
                },
                onDeselectAll: function () {
                    state.selectTag.length = 0;
                },
                onChange: function (option, checked, select) {
                    let val = parseInt($(option).val()), index = state.selectTag.indexOf(val);
                    (checked) ? state.selectTag.push(val) : (index > -1) ? state.selectTag.splice(index, 1) : state.selectTag;
                    state.selectTag.sort();
                },
                onInitialized: function () {
                    // state.TagList.forEach(function (obj) {
                    //     state.selectTag.push(obj.id);
                    // });
                }
            });
            $('#selectTag').multiselect('updateButtonText');
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
                    state.selectCurrency.length = 0;
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
                },
                onInitialized: function () {
                    state.selectCurrency.push(2);
                    state.selectCurrency.push(4);
                    state.selectCurrency.push(13);
                    state.selectCurrency.push(15);
                    state.selectCurrency.push(51);
                    state.selectCurrency.push(61);
                }
            });
            $('#selectCurrency').multiselect('updateButtonText');
        }

        getStatusList() {
            let state = this.state;
            this.buildSelectStatus();

        }

        buildSelectStatus = ()=>{
            let state = this.state;
            $("#selectStatus").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    state.selectStatus.length = 0;
                    Object.keys(defStatusType).forEach((key) => {
                        state.selectStatus.push(parseInt(key));
                    })
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
                    state.selectStatus.push(1);
                    state.selectStatus.push(2);
                    state.selectStatus.push(3);
                    state.selectStatus.push(4);
                }
            });
            $('#selectStatus').multiselect('updateButtonText');
        }

        componentDidMount() {
            let f_trigger_Start = this.f_trigger_Start.current;
            let f_trigger_End = this.f_trigger_End.current;
            let f_trigger_TagStart = this.f_trigger_TagStart.current;
            let f_trigger_TagEnd = this.f_trigger_TagEnd.current;
            let f_trigger_StatusStart = this.f_trigger_StatusStart.current;
            let f_trigger_StatusEnd = this.f_trigger_StatusEnd.current;
            
            let selectdate_end = this.selectdate_end.current;
            let selectdate_start = this.selectdate_start.current;
            let selectTagDate_end = this.selectTagDate_end.current;
            let selectTagDate_start = this.selectTagDate_start.current;
            let selectStatusDate_end = this.selectStatusDate_end.current;
            let selectStatusDate_start = this.selectStatusDate_start.current;

            //selectdate_start.value
            Calendar.setup({
                inputField: selectdate_end, // id of the input field
                ifFormat: "mm/dd/y",        // format of the input field
                button: f_trigger_End,  // trigger for the calendar (button ID)
                singleClick: true              // double-click mode
            });

            Calendar.setup({
                inputField: selectdate_start,
                ifFormat: "mm/dd/y",
                button: f_trigger_Start,
                singleClick: true
            });

            Calendar.setup({
                inputField: selectTagDate_end,
                ifFormat: "mm/dd/y",
                button: f_trigger_TagEnd,
                singleClick: true
            });

            Calendar.setup({
                inputField: selectTagDate_start,
                ifFormat: "mm/dd/y",
                button: f_trigger_TagStart,
                singleClick: true
            });

            Calendar.setup({
                inputField: selectStatusDate_end,
                ifFormat: "mm/dd/y",
                button: f_trigger_StatusEnd,
                singleClick: true
            });

            Calendar.setup({
                inputField: selectStatusDate_start,
                ifFormat: "mm/dd/y",
                button: f_trigger_StatusStart,
                singleClick: true
            });
            
            this.getSiteList();
            this.getCurrencyList();
            this.getStatusList();
            this.GetAllTagData();
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
            let TagStartDate = this.state.TagStartDate;
            let TagEndDate = this.state.TagEndDate;
            let StatusStartDate = this.state.StatusStartDate;
            let StatusEndDate = this.state.StatusEndDate;
            let Loading = this.state.Loading;
            let ExportTransferWallet = this.state.ExportTransferWallet;
            let siteList = this.state.SiteList.map(function (obj) {
                return <option key={obj.siteId} value={obj.siteId} selected="selected">{obj.siteName}</option>;
            }.bind(this));
            let tagList = this.state.TagList.map(function (obj) {
                return <option key={obj.id} value={obj.id} selected="">{obj.tagName}</option>;
            }.bind(this));
            let currencyList = Object.keys(defCurrencyType).map((key) => {
                return <option key={key} value={key} selected="selected">{defCurrencyType[key]}</option>;
            });
            let statusList = Object.keys(defStatusType).map((key) => {
                return <option key={key} value={key} selected="selected">{defStatusType[key]}</option>;
            });
            let exportTransferWallet = Object.keys(defExportTransferWalletType).map((key)=>{
                let selected = ExportTransferWallet == key ? "selected" : "";
                return (
                    <option key={key} value={key} selected={selected}>{defExportTransferWalletType[key]}</option>
                )
            })
            siteList = <select id="selectSite" name="selectSite[]" multiple="multiple">{siteList}</select>;
            tagList = <select id="selectTag" name="selectTag[]" multiple="multiple">{tagList}</select>;
            currencyList = <select id="selectCurrency" name="selectCurrency[]" multiple="multiple">{currencyList}</select>;
            statusList = <select id="selectStatus" name="selectStatus[]" multiple="multiple">{statusList}</select>;
            exportTransferWallet = <select onChange={this.onChange.bind(this)} name="ExportTransferWallet">{exportTransferWallet}</select>
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
                                    <label className="label">Affiliates Code</label>
                                    <div className="field">
                                        <input type="text" placeholder="Affiliates Code" autoComplete="off" name="AffCode" value={this.state.AffCode} onChange={this.onChange.bind(this)} />
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
                                    <label className="label">Bank Account Number</label>
                                    <div className="field">
                                        <input type="text" placeholder="Bank Account Number" autoComplete="off" name="BankAccountNumber" value={this.state.BankAccountNumber} onChange={this.onChange.bind(this)} />
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
                                <div className="input-item">
                                    <span className="label">Status</span>
                                    <div className="field">
                                        {statusList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Status Update Date (From)</span>
                                    <div className="date">
                                        <input ref={this.selectStatusDate_start} type="text" defaultValue={!isNaN(StatusStartDate) ? moment.unix(StatusStartDate).format('l') : null} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_StatusStart}>{ }</a>
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Status Update Date (To)</span>
                                    <div className="date">
                                        <input ref={this.selectStatusDate_end} type="text" defaultValue={!isNaN(StatusEndDate) ? moment.unix(StatusEndDate).format('l') : null} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_StatusEnd}>{ }</a>
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Tag Filter</span>
                                    <div className="field">
                                        {tagList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Tag Update Date (From)</span>
                                    <div className="date">
                                        <input ref={this.selectTagDate_start} type="text" defaultValue={!isNaN(TagStartDate) ? moment.unix(TagStartDate).format('l') : null} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_TagStart}> </a>
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Tag Update Date (To)</span>
                                    <div className="date">
                                        <input ref={this.selectTagDate_end} type="text" defaultValue={!isNaN(TagEndDate) ? moment.unix(TagEndDate).format('l') : null} size="8" />
                                        <a className="icon-calendar" ref={this.f_trigger_TagEnd}>{ }</a>
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Export Transfer Wallet</span>
                                    <div className="field">
                                        {exportTransferWallet}
                                    </div>
                                </div>
                                {Loading ? 
                                <div className="button-group">
                                    <button className="btn btn-search" type="submit" disabled={true}><span>Loading</span></button>
                                </div>:
                                <div className="button-group">
                                    <button type="submit" className="btn btn-primary">Search</button>
                                </div>}                       
                            </div>
                        </div>
                    </form>
                    {this._Search && this.state.searchData != null ? <CustInfoList onExport={this.onExport.bind(this)} searchData={this.state.searchData} TransId={this.state.sTransId} ChangePage={this.onSearch.bind(this)} TagList={this.state.TagList}/> : null}
                    <div style={{ display: 'none' }}>
                        {this.state.ExportParam}
                    </div>
                </div>
            )
        }
    }

    ReactDOM.render(<CustInfoPanel />, document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios, moment);