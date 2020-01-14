import {Pagination} from '../commJS/Pagination.jsx';

(function ($, React, ReactDOM, i18n, axios) {
    const CustStatusOp = {
        1:<span className="status status-active">Running</span>,
        2:<span className="status status-suspend">Suspend</span>,
        3:<span className="status status-closed">Closed</span>,
    }    

    class CustInfoRow extends React.Component {
        render() {
            let data=this.props.dataRow;
            return (
                <tr>
                    <td>{data.siteName}</td>
                    <td><a href={"/CustManage/CustInfo?UserName="+data.userName+"&custId="+ data.custId +"&TransId=" + (this.props.transId||"")} target="_blank">{data.userName}</a></td>
                    <td>{data.userLevel}</td>
                    <td>{i18n["lbl_Currency"+data.currencyId]}</td>
                    <td>{data.agent}</td>
                    <td>{data.isTest.toString()}</td>
                    <td>{CustStatusOp[data.status]}</td>
                </tr>
            )
        }
    }

    class CustInfoList extends React.Component {

        render() {
            let data = this.props.searchData&&this.props.searchData.data;
            console.log(data)
            let custInfoRows = null;
            let TransId = this.props.TransId;
            if(data!=null){
                custInfoRows = <tbody>{data.map(x => <CustInfoRow transId={TransId} key={x.CustId} dataRow={x}/>)}</tbody>;
            }

            return (
                data.Loading ? 
                <div className="text-center">
                    <div className="spinner-border" role="status">
                        <span className="sr-only">Loading...</span>
                    </div>
                </div>
                : 
                <div className="table-responsive-md">
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th scope="col">Site Name</th>
                                <th scope="col">User Name</th>
                                <th scope="col">Vip Level</th>
                                <th scope="col">Currency</th>
                                <th scope="col">Agent</th>
                                <th scope="col">IsTest</th>
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
        }

        handleSubmit(event) {
            event.preventDefault();
            this._Search = true;
            this.setState({sUserName: this.state.UserName,sTransId:this.state.TransId, Loading: true}, function() {
                this.onSearch(1);
            }.bind(this));
        }

        onSearch(pageNumber) {

            let data = {UserName: this.state.sUserName=="" ? null:this.state.sUserName, TransId: this.state.sTransId=="" ? null:this.state.sTransId,PageNumber: pageNumber, PageSize: this._PageSize};

            axios.post('/api/CustManage/GetCustInfoList', data)
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({searchData: result, Loading: false});
                } else {
                    alert(result.message);
                    this.setState({Loading: false});
                }
            }.bind(this)).catch(function (error) {
                alert(error);
                this.setState({Loading: false});
            }.bind(this));
        }

        onUserNameChange(e){
            this.setState({UserName: e.target.value});
        }

        onTransIdChange(e){
            this.setState({TransId: e.target.value});
        }

        render() {

            return (
                <div>
                    <h4 className="page-name">Customer Search</h4>

                    <form onSubmit={this.state.Loading ? null : this.handleSubmit.bind(this)}>
                        <div className="filter">
                            <div className="row-flex">
                                <div className="input-item">
                                    <label htmlFor="UserName" className="label">User Name</label>
                                    <div className="field">
                                        <input type="text" placeholder="User Name" autoComplete="off" value={this.state.UserName} onChange={this.onUserNameChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="input-item">
                                    <label htmlFor="TransId" className="label">Transaction ID</label>
                                    <div className="field">
                                        <input type="text" placeholder="Transaction ID" autoComplete="off" value={this.state.TransId} onChange={this.onTransIdChange.bind(this)} />
                                    </div>
                                </div>
                                <div className="button-group">
                                    <button type="submit" className="btn btn-primary">Search</button>
                                </div>
                            </div>
                        </div>
                    </form>
                    {this._Search && this.state.searchData !=null ? <CustInfoList searchData={this.state.searchData} TransId={this.state.sTransId} ChangePage={this.onSearch.bind(this)}/>: null}     
                </div>                
            )
        }
    }

    ReactDOM.render(<CustInfoPanel/>,document.getElementById('Panel'));
})(jQuery, React, ReactDOM, i18n, axios);