export class BankLog extends React.Component {
    constructor(props) {
        super(props);
    }
    componentDidMount() {
        this.props.GetBankLog();
    }
    render() {
        
        console.log(this.props);
        let rows = this.props.searchData.length != 0 ? this.props.searchData.map((row, idx) => {
            return (
                <tr key={"data-" + idx}>
                    <th>{row.bankName}</th>
                    <th>{row.accountName}</th>
                    <th>{row.accountNum}</th>
                    { this.props.CurrencyId == 61 ?
                        <th>{row.ifscCode}</th>
                    :null}          
                    <td>{moment(row.createTime).format('l LTS')}</td>
                </tr>
            );
        }) : null;

        return (
            <div className="table" >
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">BankName</th>
                            <th scope="col">AccountName</th>
                            <th scope="col">AccountNum</th>
                            { this.props.CurrencyId == 61 ?
                                <th scope="col">IFSC Code</th>
                            :null}
                            <th scope="col">Create Time {moment().format('(Z)')}</th>
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
