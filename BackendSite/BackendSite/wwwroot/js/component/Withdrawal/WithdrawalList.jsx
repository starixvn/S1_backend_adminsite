import { number_format } from '../commJS/common';
import { PaginationTotalAmount } from '../commJS/PaginationTotalAmount.jsx';
import * as Loading from '../commJS/Loading.jsx';
import DateTimePicker from 'react-datetime';

(function ($, React, ReactDOM, i18n, axios, moment, Calendar) {
    const WithdrawalStatus = {
        Pending: 1,
        ToKyc: 8,
        OnHold: 2,
        Verified: 3,
        Process: 4,
        Cancelled: 5,
        Approved: 6,
        Rejected: 7
    };

    const WithdrawalStatusOp = {
        1: <span className="status status-withdrawal-pending">{i18n["lbl_WithdrawalStatus1"]}</span>,
        2: <span className="status status-withdrawal-hold">{i18n["lbl_WithdrawalStatus2"]}</span>,
        3: <span className="status status-withdrawal-verified">{i18n["lbl_WithdrawalStatus3"]}</span>,
        4: <span className="status status-withdrawal-process">{i18n["lbl_WithdrawalStatus4"]}</span>,
        5: <span className="status status-withdrawal-cancelled">{i18n["lbl_WithdrawalStatus5"]}</span>,
        6: <span className="status status-withdrawal-approved">{i18n["lbl_WithdrawalStatus6"]}</span>,
        7: <span className="status status-withdrawal-rejected">{i18n["lbl_WithdrawalStatus7"]}</span>,
        8: <span className="status status-withdrawal-kyc">{i18n["lbl_WithdrawalStatus8"]}</span>,
    }

    const TypeSelectOp = {
        0: "Undefine",
        1: "Online Payment",
        2: "Local Bank",
        4: "Crypto Pay",
    };

    const StatusSelectOp = {
        1: "Pending",
        8: "To KYC",
        2: "On Hold",
        3: "Verified",
        4: "Process",
        5: "Cancelled",
        6: "Approved",
        7: "Rejected",
    }

    const LevelStatus = {
        0: 0,
        1: 1,
        2: 2,
        3: 3,
        4: 4,
        5: 5,
        6: 6
    }
    const AmountStatus = {
        0: "",
        1: "Amount Greater Than",
        2: "Amount Smaller Than"
    }
    const PageSize = {
        10: 10,
        25: 25,
        50: 50
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

        onExport() {
            this.props.onExport();
        }
        
        render() {
             let rows = this.props.searchData.data.map(row => {
                let styleblod = (row.flag === 0) ? { fontWeight: 'bold' } : {};
                //銅=>銀=>金=>白金=>水鑽藍=>紅寶石
                let vipbg = (row.vipLevel === 1 ) ? "c6842c": (row.vipLevel === 2 ) ? "c0c0c0" : (row.vipLevel === 3 ) ? "ffd700" : (row.vipLevel === 4 ) ? "66CDAA" :　(row.vipLevel === 5) ? "4169E1" : (row.vipLevel === 6) ? "B22222" : null;
                return (
                    <tr key={row.transId} style={styleblod}>
                        <th scope="row">
                            {(subFuncKeys.indexOf("f_wd_execute") > -1) ? <a href="#" role="button" onClick={this.onCheckDetail.bind(this, row.transId)}>{row.transId}</a> : row.transId}
                        </th>
                        {/* <td>{row.calCode}</td> */}
                        <td>{row.operator}</td>
                        <td>{WithdrawalStatusOp[row.status]}{row.status == 8 && row.hasPhotoId ? <span>(Uploaded PhothId)</span> : null} </td>
                        <td>{i18n["lbl_WithdrawalType" + row.withdrawalType]}</td>
                        <td>{row.calCode}</td>
                        <td>{row.isKyc === 1 ? "YES" : "NO"}</td>
                        <td>{row.isRisk === 1 ? "YES" : "NO"}</td>
                        <td><a href={"/CustManage/CustInfo?UserName=" + row.userName + "&custId=" + row.custId + "&TransId=" + row.transId} target="_blank">{row.userName}</a></td>
                        {row.vipLevel === 0 ? <td>{row.vipLevelName}</td> : <td bgcolor={vipbg}>{row.vipLevelName}</td>}
                        <td>{row.withdrawalType === 4 ? row.channelName : "" }</td>
                        <td>{JSON.parse(row.withdrawalInfo).ExchangeAmount}</td>
                        <td>{i18n["lbl_Currency" + row.currencyId]}</td>
                        <td className="text-right">{number_format(row.amount)}</td>
                        {(row.bankName == null&& row.accountType != null)?
                            <td>{row.cryptoCurrency || row.accountType}</td>:<td>{row.bankName}</td>}
                        {/* <td>{row.accountNo}</td>
                        <td>{row.accountName}</td> */}
                        <td>{moment(row.createTime).format('l LTS')}</td>
                        <td>{row.updateTime ? moment(row.updateTime).format('l LTS') : null}</td>
                        <td>{row.status ==5 || row.status==6 || row.status==7 ? moment(row.flowEndTime).format('l LTS') : null}</td>
                        <td className="text-right">{row.withdrawlCnt}</td>
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
                                <th scope="col">Operator</th>
                                <th scope="col">{i18n.lbl_Status}</th>
                                <th scope="col">{i18n.lbl_WithdrawalType}</th>
                                <th scope="col">CalCode</th>
                                <th scope="col">Kyc</th>
                                <th scope="col">Risk</th>
                                <th scope="col">{i18n.lbl_UserName}</th>
                                <th scope="col">VIP Level</th>
                                <th scope="col">NetWork</th>
                                <th scope="col">Crypto Amount</th>
                                <th scope="col">Currency</th>
                                <th scope="col" className="text-right">{i18n.lbl_Amount}</th>
                                <th scope="col">Bank Name</th>
                                {/* <th scope="col">To Acc. NO.</th>
                                <th scope="col">To Acc. Name</th> */}
                                <th scope="col">{i18n.lbl_CreateTime + " (" + moment().format('Z') + ")"}</th>
                                <th scope="col">{"Update Time (" + moment().format('Z') + ")"}</th>
                                <th scope="col">{"Flow End Time (" + moment().format('Z') + ")"}</th>
                                <th scope="col" className="text-right">Withdrawl Count</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <PaginationTotalAmount Data={{ ...this.props.searchData }} ChangePage={this.props.ChangePage} />
                </div>
            )
        }
    }

    class WithdrawalRequestComment extends React.Component {
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
                        <td>{i18n["lbl_WithdrawalStatus" + obj.action]}</td>
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

    class WithdrawalDetail extends React.Component {
        _clipboard = null;
        constructor(props) {
            super(props);
            this.state = { Data: null};
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

        onRefresh() {
            this.onGetData();
        }

        onGetData() {
            Loading.OnLoading();
            axios.post('/api/Withdrawal/GetWithdrawalRequest', { transId: this.props.transId })
                .then(async function (response) {
                    Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        //console.log(' onGetData()',result.data);
                        if (result.data.status == 3) {
                            var channelName = result.data.cryptoCurrency === result.data.network
                                ? result.data.network
                                : `${result.data.cryptoCurrency}-${result.data.network}`;
                            await this.onGetPayList(result.data.currencyId, result.data.sysId);
                            await this.onGetBankInfoOffList(result.data.currencyId, result.data.sysId, result.data.siteId);
                            await this.onGetCryptoPayList(result.data.currencyId, result.data.sysId, channelName);
                        }
                        
                        let SelId = null;
                        let ChannelId = 0;
                        if (result.data.withdrawalType == 1) {
                            SelId = result.data.payId;
                        } else if (result.data.withdrawalType == 2) {
                            SelId = result.data.bankId;
                        } else if (result.data.withdrawalType == 4) {
                            SelId = result.data.payId;
                        }
                        this.setState({ Data: result.data, selectType: result.data.withdrawalType, GetData: true, SelId: SelId, ChannelId: ChannelId, Comment: result.data.comment });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    Loading.UnLoading();
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
            axios.post('/api/Withdrawal/GetWithdrawalRequestComment', { transId: this.props.transId })
                .then(function (response) {
                    Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ Comments: result.data, GetData: true,Comment: "" });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    Loading.UnLoading();
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
                case 2:
                    confirmstring += " On Hold(Risk Check) ?";
                    break;
                case 3:
                    confirmstring += " Verified ?";
                    break;
                case 4:
                    confirmstring += " Process ?";
                    break;
                case 6:
                    confirmstring += " Approved ?";
                    break;
                case 7:
                    confirmstring += " Rejected ?";
                    break;
                case 8:
                    confirmstring += " To KYC ?";
                    break;
                default:
                    confirmstring += " ?";
                    break;
            }
            if ((action != 0 && confirm(confirmstring)) || action == 0) {
                Loading.OnLoading();
                let data = { transId: this.props.transId, Comment: this.state.Comment, Action: action };
                if (action == 4) {
                    data.WithdrawalInfo = this.state.Data.withdrawalInfo;
                    data.SelId = parseInt(this.state.SelId);
                    data.WithdrawalType = this.state.selectType;
                    if (this.state.selectType == 4)
                    {
                        data.ChannelId = this.state.ChannelId;
                        if((data.ChannelId == 0 || data.ChannelId == null) && this.state.CryptoPayList != null &&　data.SelId != 0)
                            data.ChannelId = this.state.CryptoPayList.find(x=>x.payId == data.SelId).channelId;
                    }

                }
                axios.post('/api/Withdrawal/WithdrawalConfirm', data)
                    .then(function (response) {
                        Loading.UnLoading();
                        let result = response.data;
                        if (result.errorCode == 0) {
                            //alert(result.message);
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

        onSelectType(event) {
            let SelId = null;
            let ChannelId = 0;
            if (parseInt(event.target.value) == 1 && this.state.PayList.length > 0) {
                SelId = this.state.PayList[0].payId;
            } else if (parseInt(event.target.value) == 2 && this.state.BankInfoOffList.length > 0) {
                SelId = this.state.BankInfoOffList[0].bankId;
            } else if (parseInt(event.target.value) == 4 && this.state.CryptoPayList.length > 0) {
            //   console.log(this.state.CryptoPayList);
                SelId = this.state.CryptoPayList[0].payId;
                ChannelId = this.state.CryptoPayList[0].channelId;
            }
            this.setState({ selectType: parseInt(event.target.value), SelId: SelId, ChannelId: ChannelId });
        }

        async onGetBankInfoOffList(currencyId, sysId, siteId) {
            let data = this.state.Data;
            await axios.post('/api/Withdrawal/GetBankInfoOffList', { CurrencyId: currencyId, SysId: sysId, SiteId: siteId })
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({...Comment,
                             BankInfoOffList: result.data });
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
            await axios.post('/api/Withdrawal/GetPayList', { CurrencyId: currencyId, SysId: sysId })
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ ...Comment,PayList: result.data });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
        }

        async onGetCryptoPayList(currencyId, sysId, channelName ) {
            let data = this.state.Data;
            await axios.post('/api/Withdrawal/GetCryptoPayList', { CurrencyId: currencyId, SysId: sysId, ChannelName: channelName})
                .then(function (response) {
                    //Loading.UnLoading();
                    let result = response.data;
                    if (result.errorCode == 0) {
                        this.setState({ CryptoPayList: result.data });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    //Loading.UnLoading();
                    alert(error);
                }.bind(this));
        }

        onSelId(event) {
            
            let ChannelId = 0;
            if(this.state.selectType == 4)
            {
                ChannelId = this.state.CryptoPayList.find(x=>x.payId == event.target.value).channelId;
            }
            //console.log('event',event,'ChannelId',ChannelId);
            this.setState({ SelId: event.target.value, ChannelId: ChannelId });
        }

        handlePaste = (event) => {
            let transId = this.props.transId;
            let imgindex = 0;
            if (event.clipboardData.files.length) {
                for(var i=0;i<event.clipboardData.items.length;i++){
                    if(event.clipboardData.items[i].type == "image/png"){
                        imgindex = i;                     
                    }
                }
            }
            var blob = event.clipboardData.items[imgindex].getAsFile();
            var reader = new FileReader();
            const scope = this;
            reader.onload = function(event){
                axios.post('/api/Withdrawal/UploadWithdrawalPic', { Pic: event.target.result,TransId: transId })
                .then(async function (response) {
                    if(response.data.errorCode == 0){
                        scope.onRefresh();                    
                    }
                    else{
                    }
                    
                }.bind(this)).catch(function (error) {
                    
                }.bind(this));
            }; // data url!

            reader.readAsDataURL(blob)         
        }

        onDelete = ()=>{
            let transId = this.props.transId;
            axios.post('/api/Withdrawal/DeleteWithdrawalPic', { TransId: transId })
                .then(async function (response) {
                    if(response.data.errorCode == 0){
                        this.onRefresh();
                    }
                    else{
                    }
                    
                }.bind(this)).catch(function (error) {
                    
                }.bind(this));               
        }

         render() {
            // console.log("CryptoPayList",this.state.CryptoPayList);
            let data = this.state.Data;
            let btnGroup = [];
            let selectType = null
            let selectPay = null;
            let PayFrom = [];
            if (data != null) {
                // if (data.status == 1 || data.status == 2 || data.status == 3 || data.status == 4 || data.status == 8)
                    btnGroup.push(<button key={0} type="button" className="btn btn-withdrawal-draft" onClick={this.onConfirm.bind(this, 0)}>Draft</button>);
                if ((data.status == 1  && subFuncKeys.indexOf("f_wd_risk") > -1))
                    btnGroup.push(<button key={2} type="button" className="btn btn-withdrawal-hold" onClick={this.onConfirm.bind(this, 2)}>On Hold(Risk Check)</button>);
                if ((data.status == 1 && subFuncKeys.indexOf("f_wd_risk") > -1) || (data.status == 4 && data.sameOperator) || (data.status == 2 && subFuncKeys.indexOf("f_wd_risk") > -1))
                    btnGroup.push(<button key={3} type="button" className="btn btn-withdrawal-verified" onClick={this.onConfirm.bind(this, 3)}>Verified</button>);
                if (data.status == 3 && subFuncKeys.indexOf("f_wd_payment") > -1)
                    btnGroup.push(<button key={4} type="button" className="btn btn-withdrawal-process" onClick={this.onConfirm.bind(this, 4)}>Process</button>);
                if (data.status == 4 && data.withdrawalType != 1 && data.sameOperator)
                    btnGroup.push(<button key={6} type="button" className="btn btn-withdrawal-aprroved" onClick={this.onConfirm.bind(this, 6)}>Approved</button>);
                if ((data.status == 3 && subFuncKeys.indexOf("f_wd_payment") > -1) || (data.status == 8 && (subFuncKeys.indexOf("f_wd_risk") > -1 || subFuncKeys.indexOf("f_wd_payment") > -1)) 
                    || (data.status == 1 && subFuncKeys.indexOf("f_wd_risk") > -1) || (data.status == 2 && subFuncKeys.indexOf("f_wd_risk") > -1))
                    btnGroup.push(<button key={7} type="button" className="btn btn-withdrawal-rejected" onClick={this.onConfirm.bind(this, 7)}>Rejected</button>);
                if ((data.status == 8 && (subFuncKeys.indexOf("f_wd_risk") > -1 || subFuncKeys.indexOf("f_wd_payment") > -1)) 
                    || (data.status == 3 && subFuncKeys.indexOf("f_wd_payment") > -1))
                    btnGroup.push(<button key={1} type="button" className="btn btn-withdrawal-pending" onClick={this.onConfirm.bind(this, 1)}>Pending</button>);
                if (((data.status == 1 || data.status == 2) && subFuncKeys.indexOf("f_wd_risk") > -1))
                    btnGroup.push(<button key={8} type="button" className="btn btn-withdrawal-kyc" onClick={this.onConfirm.bind(this, 8)}>ToKyc</button>);

                selectType = i18n["lbl_WithdrawalType" + data.withdrawalType];

                if (data.status == 3) {
                    let selectOp = [];
                    selectOp.push(<option key={0} value="0">Undefine</option>);
                    if(this.state.Data.accountType === 'crypto')
                    {
                        if (this.state.CryptoPayList.length > 0)
                            selectOp.push(<option key={4} value="4">Crypto Pay</option>);
                    }else
                    {
                        if (this.state.PayList.length > 0)
                            selectOp.push(<option key={1} value="1">Online Payment</option>);
                        if (this.state.BankInfoOffList.length > 0)
                            selectOp.push(<option key={2} value="2">Local Bank</option>);
                    }
                    selectType = <select className="form-control" onChange={this.onSelectType.bind(this)} value={this.state.selectType}>
                        {selectOp}
                    </select>;

                    selectPay = null;
                    if (this.state.selectType == 1) {
                        selectPay = this.state.PayList.map(row => <option key={row.payId} value={row.payId}>{row.payName}</option>);
                        /*if(this.state.PayList.length>0) {
                            this.state.SelId=this.state.PayList[0];
                        }*/
                    } else if (this.state.selectType == 2) {
                        selectPay = this.state.BankInfoOffList.map(row => <option key={row.bankId} value={row.bankId}>{row.bankCode + "-" + row.accountName + "-" + row.accountNo}</option>);
                        /*if(this.state.BankInfoOffList.length>0) {
                            this.state.SelId=this.state.BankInfoOffList[0];
                        }*/
                    } else if (this.state.selectType == 4) {
                        selectPay = this.state.CryptoPayList.map(row => <option key={row.payId} value={row.payId}>{row.payName}</option>);
                        /*if(this.state.BankInfoOffList.length>0) {
                            this.state.SelId=this.state.BankInfoOffList[0];
                        }*/
                    }

                    if (selectPay != null) {
                        selectPay = <tr>
                            <th scope="row">select Pay</th>
                            <td><select className="form-control" value={this.state.SelId} onChange={this.onSelId.bind(this)}>{selectPay}</select></td>
                        </tr>;
                    }
                } else {
                    if (data.payId > 0) {
                        PayFrom.push(<tr key={1}>
                            <th scope="row">Payment</th>
                            <td>{data.payName}</td>
                        </tr>);
                    } else if (data.bankId > 0) {
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
            let picrows = null;
            if (data != null) {
                if(data.hasPic){
                    picrows = 
                    <div>
                        <img className="img-slip" src={"/api/Withdrawal/GetWithdrawalPic?transId=" + data.transId + "&BankSlipPicCount=" + data.bankSlipPicCount} />
                        <div><button type="button" className="btn btn-back pull-left" onClick={this.onDelete.bind(this)}>Delete</button></div>                     
                    </div>;
                }
                else{
                    if(data.status == 4){
                        picrows = <div
                                    onPaste={this.handlePaste.bind(this)}>
                                        <div
                                            style={{
                                                marginTop: 5,
                                                padding: 10,
                                                fontStyle: "italic",
                                                color: "red",
                                                border: "1px solid red",
                                                height: 500,
                                            }}
                                        >
                                            Paste Area
                                        </div>
                                </div>
                    }
                }
            }
//console.log('return',data);
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
                                <th scope="row">Kyc</th>
                                <td>{data.isKyc === 1 ? "YES" : "NO"}</td>
                            </tr>
                            <tr>
                                <th scope="row">Risk</th>
                                <td>{data.isRisk === 1 ? "YES" : "NO"}</td>
                            </tr>
                            <tr>
                                <th scope="row">Username</th>
                                <td><a href={"/CustManage/CustInfo?UserName=" + data.userName + "&custId=" + data.custId + "&TransId=" + data.transId} target="_blank">{data.userName}</a></td>
                            </tr>
                            <tr>
                                <th scope="row">E-mail</th>
                                <td>{data.email}</td>
                            </tr>
                            <tr>
                                <th scope="row">{"Date" + " (" + moment().format('Z') + ")"}</th>
                                <td>{moment(data.createTime).format('l LTS')}</td>
                            </tr>
                            {data.bankName != null ?
                            <tr>
                                <th scope="row">Name of Bank</th>
                                <td>{data.bankName}</td>
                            </tr>
                            : null}
                            {data.accountType != null ?
                            <tr>
                                <th scope="row">Account Type</th>
                                <td>{data.cryptoCurrency || data.accountType}</td>
                            </tr>
                            : null}
                            {data.accountType != null ?
                            <tr>
                                <th scope="row">Network</th>
                                <td>{data.network}</td>
                            </tr>
                            : null}
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
                            {data.currencyId == 61 ?
                            <tr>
                                <th scope="row">IFSC Code</th>
                                <td>
                                    {data.ifscCode}
                                    <button type="button" className="btn btn-copy js-copy" data-clipboard-text={data.ifscCode}>Copy</button>
                                </td>
                            </tr>
                            : null}
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
                            <tr>
                                <th scope="row">Withdrawl Count</th>
                                <td>{data.withdrawlCnt}</td>
                            </tr>
                            <tr>
                                <th scope="row">{"Update Time (" + moment().format('Z') + ")"}</th>
                                <td>{data.updateTime ? moment(data.updateTime).format('l LTS') : null}</td>
                            </tr>
                            {data.status == 5 || data.status == 6 || data.status == 7 ?
                            <tr>
                                <th scope="row">{"Flow End Time (" + moment().format('Z') + ")"}</th>
                                <td>{data.flowEndTime ? moment(data.flowEndTime).format('l LTS') : null}</td>
                            </tr>
                            : null}
                        </tbody>
                    </table>
                    {picrows}
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
        _timeInterval = 15;
        constructor(props) {
            super(props);
            this.selectdate_start = React.createRef();
            this.selectdate_end = React.createRef();
            this.ExportForm = React.createRef();
            this.state = { SiteList: [], selectSite: [], selectType: [], selectCurrency: [], selectStatus: [], SelectUser: "", SelectWithdrawalID: "", ShowDetial: false, timeInterval: this._timeInterval, IsRefresh: false, MutiSelRefresh: false ,levelStatus: [],amountType:0,
            amount: "",pageSize:25};
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
            let data = { StartDate: this.searchFilter.StartDate, EndDate: this.searchFilter.EndDate, PageNumber: pageNumber, SelectSite: this.searchFilter.selectSite, 
                        AmountType: parseInt(this.searchFilter.amountType), Amount:this.searchFilter.amount,PageSize:this.searchFilter.pageSize};

            if (this.searchFilter.SelectWithdrawalID != "") {
                data.WithdrawalId = this.searchFilter.SelectWithdrawalID;
            }

            if (this.searchFilter.selectType.length != 0) {
                data.withdrawalType = this.searchFilter.selectType;
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

            if (this.searchFilter.levelStatus.length != 0) {
                data.LevelStatus = this.searchFilter.levelStatus;
            }
            
            axios.post('/api/Withdrawal/GetWithdrawalRequestList', data)
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
            let data = { StartDate: this.searchFilter.StartDate, EndDate: this.searchFilter.EndDate, SelectSite: this.searchFilter.selectSite, AmountType: parseInt(this.searchFilter.amountType), Amount:this.searchFilter.amount };
            if (this.searchFilter.SelectWithdrawalID != "") {
                data.WithdrawalId = this.searchFilter.SelectWithdrawalID;
            }

            if (this.searchFilter.selectType.length != 0) {
                data.withdrawalType = this.searchFilter.selectType;
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

            if (this.searchFilter.levelStatus.length != 0) {
                data.LevelStatus = this.searchFilter.levelStatus;
            }

            let ExportParam = [];
            for (var prop in data) {
                ExportParam.push(<input key={prop} type="hidden" name={prop} value={data[prop]} />);
            }

            ExportParam = <form method="post" action={'/Finance/ExportWithdrawalList'} ref={this.ExportForm}>{ExportParam}</form>;

            this.setState({ ExportParam, ExportParam }, function () {
                this.ExportForm.current.submit();
            }.bind(this));
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

            if(parseInt(this.state.amountType) != 0 && this.state.amount == ""){
                alert("Please Set Amount")
                return;
            }

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
                            state.selectSite.push(obj.siteId);
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

        //levelStatus
        buildLevel = () => {
            let state = this.state, searchFilter = this.searchFilter;
            $("#levelStatus").multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Check all!',
                onSelectAll: function () {
                    state.levelStatus.length = 0;
                    Object.keys(LevelStatus).forEach(function (obj) {
                        state.levelStatus.push(parseInt(obj));
                    });
                },
                onDeselectAll: function () {
                    state.levelStatus.length = 0;
                },
                onChange: function (option, checked, select) {
                    let val = parseInt($(option).val()), index = state.levelStatus.indexOf(val);
                    (checked) ? state.levelStatus.push(val) : (index > -1) ? state.levelStatus.splice(index, 1) : state.levelStatus;
                    state.levelStatus.sort();
                },
                onInitialized: function () {
                    state.levelStatus.length = 0;
                    (searchFilter.levelStatus) ?
                        searchFilter.levelStatus.forEach(function (obj) {
                            state.levelStatus.push(parseInt(obj));
                        })
                        : Object.keys(LevelStatus).forEach(function (obj) {
                            state.levelStatus.push(parseInt(obj));
                        });
                }
            });
            (state.MutiSelRefresh) ? $('#levelStatus').multiselect('select', searchFilter.levelStatus) : $('#levelStatus').multiselect('selectAll', false);
            $('#levelStatus').multiselect('updateButtonText');
        }
        componentDidUpdate() {
            if (this.state.MutiSelRefresh) {
                this.buildSelectSite();
                this.buildTypeSite();
                this.buildCurrencySite();
                this.buildStatusSite();
                this.buildLevel();
                this.setState({ MutiSelRefresh: false });
            }
        }

        componentDidMount() {
            this.getSiteList();
            this.buildTypeSite();
            this.buildCurrencySite();
            this.buildStatusSite();
            this.buildLevel();
            //setTimeout(thisAutoRefresh, 2000);
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

            let levelList = Object.keys(LevelStatus).map(function (obj) {
                return <option key={`status-list-${obj}`} value={obj}>{LevelStatus[obj]}</option>;
            }.bind(this));
            levelList = <select id="levelStatus" name="levelStatus[]" multiple="multiple">{levelList}</select>;

            let AmountList = Object.keys(AmountStatus).map(function (obj) {
                return <option key={`status-list-${obj}`} value={obj}>{AmountStatus[obj]}</option>;
            }.bind(this));
            AmountList = <select onChange={this.onChange.bind(this)} name="amountType" value={this.state.amountType}>{AmountList}</select>

            let PageSizeList = Object.keys(PageSize).map(function (obj) {
                return <option key={`status-list-${obj}`} value={obj}>{PageSize[obj]}</option>;
            }.bind(this));
            PageSizeList = <select onChange={this.onChange.bind(this)} name="pageSize" value={this.state.pageSize}>{PageSizeList}</select>

            let searchData = this.state.searchData;
            let StartDate = this.state.StartDate;
            let EndDate = this.state.EndDate;
            let IsRefresh = this.state.IsRefresh;

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
                                <div className="input-item">
                                    <span className="label">VIP Level</span>
                                    <div className="field">
                                        {levelList}
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
                                <div className="input-item">
                                    <span className="label">Amount Type</span>
                                    <div className="field">
                                        {AmountList}
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Amount</span>
                                    <div className="field">
                                        <input name="amount" value={this.state.amount} type="number" onChange={this.onChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <span className="label">Page Size</span>
                                    <div className="field">
                                        {PageSizeList}
                                    </div>
                                </div>
                                {IsRefresh ?
                                <div className="button-group">
                                    <button className="btn btn-search" type="submit" disabled={true}><span>Loading</span></button>
                                </div>:
                                <div className="button-group">
                                    <button className="btn btn-search" type="submit"><span>Search</span></button>
                                </div>}
                            </div>
                        </div>
                    </form> : null}
                    {searchData != null && !this.state.ShowDetial ? <WithdrawalList onExport={this.onExport.bind(this)} timeInterval={this.state.timeInterval} searchData={searchData} ChangePage={this.onSearch.bind(this)} onCheckDetail={this.onCheckDetail.bind(this)} onRefresh={!this.state.IsRefresh ? this.onRefresh.bind(this) : null} /> : null}
                    {this.state.transId != null && this.state.ShowDetial ? <WithdrawalDetail transId={this.state.transId} onBack={this.onBack.bind(this)} /> : null}
                    <div style={{ display: 'none' }}>
                        {this.state.ExportParam}
                    </div>
                </div>
            )
        }
    }
    ReactDOM.render(<WithdrawalPanel />, document.getElementById('searchForm'));
})(jQuery, React, ReactDOM, i18n, axios, moment);
