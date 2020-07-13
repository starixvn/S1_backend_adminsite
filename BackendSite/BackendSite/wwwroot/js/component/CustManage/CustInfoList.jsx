import {Pagination} from '../commJS/Pagination.jsx';
import * as Loading from '../commJS/Loading.jsx';

(function ($, React, ReactDOM, i18n, axios, moment) {
    const CustStatusOp = {
        1:<span className="status status-active">Running</span>,
        2:<span className="status status-suspend">Suspend</span>,
        3:<span className="status status-closed">Closed</span>,
    }    

    class CustInfoRow extends React.Component {
        render() {
            let data=this.props.dataRow;
            return (
                <tr key={data.custId}>
                    <td>{data.siteName}</td>
                    <td><a href={"/CustManage/CustInfo?UserName="+data.userName+"&custId="+ data.custId +"&TransId=" + (this.props.transId||"")} target="_blank">{data.userName}</a></td>
                    <td>{data.userLevel}</td>
                    <td>{i18n["lbl_Currency"+data.currencyId]}</td>
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
            let data = this.props.searchData&&this.props.searchData.data;
            let custInfoRows = null;
            let TransId = this.props.TransId;
            if(data!=null){
                custInfoRows = <tbody>{data.map(x => <CustInfoRow transId={TransId} key={x.custId} dataRow={x}/>)}</tbody>;
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
                    <div className="table-options">
                       <button type="button" className="btn btn-approved" onClick={this.onExport.bind(this)}>Export Excel</button>
                    </div>
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
                    <Pagination Data={{...this.props.searchData}} ChangePage={this.props.ChangePage}/>
                </div>
            )
        }
    }

    class CustInfoPanel extends React.Component {
        _PageSize = 10;
        _Search = false;
        constructor(props) {
            super(props);
            this.state = { searchData : null, UserName: "", TransId: "", Loading: false};
            
            this.ExportForm = React.createRef();
        }

        handleSubmit(event) {
            event.preventDefault();
            this._Search = true;
            this.setState({sUserName: this.state.UserName, 
                sTransId:this.state.TransId, 
                sTpUserName: this.state.TpUserName, 
                sAccountName: this.state.AccountName, 
                sLoginIp: this.state.LoginIp,                                 
                sEmail: this.state.Email, 
                sPhoneNo: this.state.PhoneNo, 
                Loading: true}, function() {
                this.onSearch(1);
            }.bind(this));
        }

        onSearch(pageNumber) {
            Loading.OnLoading();
            let data = {UserName: this.state.sUserName=="" ? null:this.state.sUserName, 
                TransId: this.state.sTransId=="" ? null:this.state.sTransId, 
                TpUserName: this.state.sTpUserName=="" ? null:this.state.sTpUserName, 
                AccountName: this.state.sAccountName=="" ? null : this.state.sAccountName, 
                LoginIp: this.state.sLoginIp=="" ? null : this.state.sLoginIp,                
                Email: this.state.sEmail=="" ? null:this.state.sEmail, 
                PhoneNo: this.state.sPhoneNo=="" ? null:this.state.sPhoneNo, 
                PageNumber: pageNumber, 
                PageSize: this._PageSize};

            axios.post('/api/CustManage/GetCustInfoList', data)
            .then(function (response) {
                Loading.UnLoading();
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({searchData: result, Loading: false});
                } else {
                    alert(result.message);
                    this.setState({Loading: false});
                }
            }.bind(this)).catch(function (error) {
                Loading.UnLoading();
                alert(error);
                this.setState({Loading: false});
            }.bind(this));
        }

        onExport() {
            let data = {UserName: this.state.sUserName=="" ? null : this.state.sUserName, 
                TransId: this.state.sTransId=="" ? null : this.state.sTransId, 
                AccountName: this.state.sAccountName=="" ? null : this.state.sAccountName, 
                LoginIp: this.state.sLoginIp=="" ? null : this.state.sLoginIp,
                TpUserName: this.state.sTpUserName=="" ? null:this.state.sTpUserName,
                Email: this.state.sEmail=="" ? null:this.state.sEmail, 
                PhoneNo: this.state.sPhoneNo=="" ? null:this.state.sPhoneNo,             
            };

            let ExportParam = [];
             for (var prop in data) {
                 ExportParam.push(<input key={prop} type="hidden" name={prop} value={data[prop]}/>);
            }

            ExportParam = <form method="post" action={'/CustManage/ExportCustInfoList'} ref={this.ExportForm}>{ExportParam}</form>;

            this.setState({ExportParam,ExportParam},function (){
                this.ExportForm.current.submit();
            }.bind(this));

            //axios.post('/Finance/ExportDepositList', data, {responseType: 'blob'});
        }
        onUserNameChange(e){
            this.setState({UserName: e.target.value});
        }

        onTransIdChange(e){
            this.setState({TransId: e.target.value});
        }

        onChange(event) {
            let obj={};
            obj[event.target.name]=event.target.value;
            this.setState(obj);
        }

        render() {

            return (
                <div>
                    <h4 className="page-name">Customer Search</h4>

                    <form onSubmit={this.state.Loading ? null : this.handleSubmit.bind(this)}>
                        <div className="filter">
                            <div className="row-flex">
                                <div className="input-item">
                                    <label className="label">User Name</label>
                                    <div className="field">
                                        <input type="text" placeholder="User Name" autoComplete="off" value={this.state.UserName} onChange={this.onUserNameChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label className="label">Real Name</label>
                                    <div className="field">
                                        <input type="text" placeholder="Real Name" autoComplete="off" name="AccountName" value={this.state.AccountName} onChange={this.onChange.bind(this)} />
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
                                    <label className="label">Saba ID</label>
                                    <div className="field">
                                        <input type="text" placeholder="Saba ID" autoComplete="off" name="TpUserName" value={this.state.TpUserName} onChange={this.onChange.bind(this)} />
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
                                <div className="button-group">
                                    <button type="submit" className="btn btn-primary">Search</button>
                                </div>
                            </div>
                        </div>
                    </form>
                    {this._Search && this.state.searchData !=null ? <CustInfoList onExport={this.onExport.bind(this)} searchData={this.state.searchData} TransId={this.state.sTransId} ChangePage={this.onSearch.bind(this)}/>: null}     
                    <div style={{display: 'none'}}>
                        {this.state.ExportParam}
                    </div>                    
                </div>                
            )
        }
    }

    ReactDOM.render(<CustInfoPanel/>,document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios, moment);