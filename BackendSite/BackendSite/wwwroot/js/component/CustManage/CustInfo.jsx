(function ($, React, ReactDOM, i18n, axios) {
    class CustInfoRow extends React.Component {
        render() {
            let data=this.props.dataRow;
            return (
                <tr>
                    <td>{data.UserName}</td>
                    <td>{data.UserLevel}</td>
                    <td>{data.CurrencyId}</td>
                    <td>{data.Agent}</td>
                    <td>{data.IsTest.toString()}</td>
                    <td>{data.Status}</td>
                </tr>
            )
        }
    }

    class CustInfoList extends React.Component {
        ChangePage(pageNumber) {
            this.props.doSearch(pageNumber);
        }

        render() {
            let data = this.props.Data;
            let custInfoRows = null;
            let PageNum=[];
            if(data!=null && data.CustInfoList!=null){
                custInfoRows = <tbody>{data.CustInfoList.map(x => <CustInfoRow key={x.CustId} dataRow={x}/>)}</tbody>;

                for(let i = 1; i<=data.TotalPages; i++){
                PageNum.push(<li className={"page-item"+(data.PageNumber==i?" active":"")} key={i}><a className="page-link" href="javascript:void(0);" onClick={data.PageNumber==i ? null: this.ChangePage.bind(this, i)}>{i}</a></li>);
                }
            }
            
            let disableNext=data.PageNumber==data.TotalPages||data.TotalPages==0;

            return (
                <div className="table-responsive-md">
                    <table className="table table-striped">
                        <thead>
                            <tr>
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

                    <nav aria-label="Page navigation example">
                        <ul className="pagination justify-content-center">
                            <li className={"page-item"+ (data.PageNumber==1?" disabled":"")}>
                                <a className="page-link" href="javascript:void(0);" tabIndex="-1" aria-disabled={data.PageNumber==1} onClick={data.PageNumber==1 ? null: this.ChangePage.bind(this, data.PageNumber-1)}>Previous</a>
                            </li>
                            {PageNum}
                            <li className={"page-item"+ (disableNext?" disabled":"")}>
                                <a className="page-link" href="javascript:void(0);" aria-disabled={disableNext} onClick={disableNext ? null: this.ChangePage.bind(this, data.PageNumber+1)}>Next</a>
                            </li>
                        </ul>
                    </nav>
                </div>
            )
        }
    }

    class CustInfoPanel extends React.Component {
        _PageSize = 10;

        constructor(props) {
            super(props);
            this.state = { CustInfoList: null, UserName: "", TransId: "", search: false};
        }

        handleSubmit(event) {
            event.preventDefault();
            this.setState({search: true, PageSize: this._PageSize, sUserName: this.state.UserName,sTransId:this.state.TransId}, function() {
                this.doSearch(1);
              }.bind(this));
        }

        doSearch(pageNumber) {

            let data = {UserName: this.state.sUserName=="" ? null:this.state.sUserName, TransId: this.state.sTransId=="" ? null:this.state.sTransId,PageNumber: pageNumber, PageSize: this.state.PageSize};

            axios.post('/api/CustManage/GetCustInfoList', data)
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    this.setState({CustInfoList: result.data, PageNumber: result.pageNumber, TotalPages: result.totalPages});
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                alert(error);
            });
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
                    <h4>Customer Information</h4>
                    <form className="shadow p-3 mb-5 bg-white rounded" onSubmit={this.handleSubmit.bind(this)}>
                        <div className="form-row">
                            <div className="form-group col-md-6">
                                <label htmlFor="UserName">User Name</label>
                                <input type="text" className="form-control" placeholder="User Name" autoComplete="off" value={this.state.UserName} onChange={this.onUserNameChange.bind(this)}/>
                            </div>
                            <div className="form-group col-md-6">
                                <label htmlFor="TransId">Transaction Id</label>
                                <input type="text" className="form-control" placeholder="Transaction Id" autoComplete="off" value={this.state.TransId} onChange={this.onTransIdChange.bind(this)}/>
                            </div>
                        </div>
                        <button type="submit" className="btn btn-primary">Search</button>
                    </form>
                    {this.state.search ? <CustInfoList Data={{...this.state}} doSearch={this.doSearch.bind(this)}/>: null}     
                </div>                
            )
        }
    }

    ReactDOM.render(<CustInfoPanel/>,document.getElementById('CustInfoPanel'));
})(jQuery, React, ReactDOM, i18n, axios);