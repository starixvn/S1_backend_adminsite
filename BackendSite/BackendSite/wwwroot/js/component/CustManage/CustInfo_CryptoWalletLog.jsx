export class CryptoWalletLog extends React.Component {
    constructor(props) {
        super(props);
    }
    componentDidMount() {
        this.props.GetCryptoWalletLog();
    }
    render() {
        console.log(this.props);
        let rows = this.props.searchData.length != 0 ? this.props.searchData.map((row, idx) => {
            return (
                <tr key={"data-" + idx}>
                    <th>USDT</th>
                    <th>{row.netWork}</th>
                    <th>{row.usdtAddress}</th>
                    <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                </tr>
            );
        }) : null;

        return (
            <div className="table" >
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">Type</th>
                            <th scope="col">NetWork</th>
                            <th scope="col">Wallet Address</th>
                            <th scope="col">Create Time (Adjusted to GMT+8)</th>
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
